using Application.Dtos.Products.ClientSide;
using Application.Dtos.SendSms.ClientSide;
using Application.Services.Interfaces;
using Domain.Enums.GeneralInfo;

namespace Web.BackgroundServices
{
    public class PriceMonitorWorker : BackgroundService
    {
        #region Fields
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PriceMonitorWorker> _logger;
        private readonly Random _random = new();
        private int _requestCount = 0;
        #endregion

        #region Ctor
        public PriceMonitorWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<PriceMonitorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Price Monitor Worker Started.");

            var cycleInterval = TimeSpan.FromMinutes(15);

            while (!stoppingToken.IsCancellationRequested)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    _logger.LogInformation("New monitoring cycle started at: {Time}", DateTimeOffset.Now);

                    await RunMonitoringCycleAsync(stoppingToken);

                    watch.Stop();

                    var sleepTime = cycleInterval > watch.Elapsed
                        ? cycleInterval - watch.Elapsed
                        : TimeSpan.Zero;

                    _logger.LogInformation(
                        "Cycle finished in {Duration} min. Sleeping for {Sleep} min.",
                        watch.Elapsed.TotalMinutes.ToString("F1"),
                        sleepTime.TotalMinutes.ToString("F1"));

                    if (sleepTime > TimeSpan.Zero)
                        await Task.Delay(sleepTime, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Price Monitor Worker was canceled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in monitoring cycle. Retrying in 5 minutes.");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task RunMonitoringCycleAsync(CancellationToken ct)
        {
            List<ProductMonitoringTargetDto> products;

            using (var scope = _scopeFactory.CreateScope())
            {
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                products = await productService.GetAllProductsForMonitoringAsync(ct);
            }

            if (products.Count == 0)
            {
                _logger.LogInformation("No eligible products found for monitoring.");
                return;
            }

            _logger.LogInformation("Monitoring {Count} products.", products.Count);

            // key: userId
            var userNotifications = new Dictionary<int, ClientSideUserChangeNotification>();

            _requestCount = 0;
            bool shouldAbortCycle = false;

            foreach (var product in products)
            {
                if (ct.IsCancellationRequested || shouldAbortCycle)
                    break;

                try
                {
                    await ThrottleAsync(ct);

                    using var productScope = _scopeFactory.CreateScope();

                    var scraperService = productScope.ServiceProvider.GetRequiredService<IDigiKalaService>();
                    var productService = productScope.ServiceProvider.GetRequiredService<IProductService>();

                    var scrapeResult = await scraperService.ScrapeFullProductAsync(product.DigiKalaId, ct);

                    if (scrapeResult.IsFailure)
                    {
                        if (!string.IsNullOrWhiteSpace(scrapeResult.Message) &&
                            scrapeResult.Message.Contains("RATE_LIMITED"))
                        {
                            _logger.LogCritical(
                                "Rate limit detected while scraping DigiKalaId: {DigiKalaId}. Aborting cycle to protect IP.",
                                product.DigiKalaId);

                            shouldAbortCycle = true;
                            break;
                        }

                        _logger.LogWarning(
                            "Scrape failed for DigiKalaId: {DigiKalaId}. Error: {Error}",
                            product.DigiKalaId,
                            scrapeResult.Message);

                        continue;
                    }

                    var affectedNotifications = await productService.SyncProductAndDetectChangesAsync(
                        product,
                        scrapeResult.Value,
                        ct);

                    foreach (var notification in affectedNotifications)
                    {
                        if (userNotifications.TryGetValue(notification.UserId, out var existing))
                        {
                            if (notification.HasPriceChange)
                                existing.HasPriceChange = true;

                            if (notification.HasBuyBoxChange)
                                existing.HasBuyBoxChange = true;
                        }
                        else
                        {
                            userNotifications[notification.UserId] = new ClientSideUserChangeNotification
                            {
                                UserId = notification.UserId,
                                HasPriceChange = notification.HasPriceChange,
                                HasBuyBoxChange = notification.HasBuyBoxChange
                            };
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while processing product ProductId: {ProductId}, DigiKalaId: {DigiKalaId}",
                        product.ProductId,
                        product.DigiKalaId);

                    continue;
                }
            }

            if (ct.IsCancellationRequested || userNotifications.Count == 0)
            {
                _logger.LogInformation("No users need notification in this cycle.");
                return;
            }

            _logger.LogInformation("Sending SMS to {Count} unique users.", userNotifications.Count);

            using (var scope = _scopeFactory.CreateScope())
            {
                var smsService = scope.ServiceProvider.GetRequiredService<ISendSmsService>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var userContacts = await userService.GetUserContactsAsync(userNotifications.Keys.ToList(), ct);

                foreach (var user in userContacts)
                {
                    if (ct.IsCancellationRequested)
                        break;

                    if (!userNotifications.TryGetValue(user.UserId, out var notification))
                        continue;

                    SmsPanelTypeEnum? smsType = (notification.HasPriceChange, notification.HasBuyBoxChange) switch
                    {
                        (true, true) => SmsPanelTypeEnum.PriceAndBuyBoxAlert,
                        (true, false) => SmsPanelTypeEnum.PriceAlert,
                        (false, true) => SmsPanelTypeEnum.BuyBoxAlert,
                        _ => null
                    };

                    if (smsType is null)
                        continue;

                    try
                    {
                        await smsService.ClientSideSendPriceAlert(new ClientSideSendPriceAlertDto
                        {
                            Mobile = user.Mobile,
                            FirstName = user.FirstName,
                            Type = smsType.Value
                        });

                        await Task.Delay(_random.Next(200, 600), ct);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send SMS to mobile: {Mobile}", user.Mobile);
                    }
                }
            }
        }

        private async Task ThrottleAsync(CancellationToken ct)
        {
            _requestCount++;

            if (_requestCount % 25 == 0)
            {
                var longPause = _random.Next(4000, 7000);

                _logger.LogInformation(
                    "Taking a longer pause of {Pause} ms after {Count} requests.",
                    longPause,
                    _requestCount);

                await Task.Delay(longPause, ct);
                return;
            }

            var delay = _random.Next(800, 1800);
            await Task.Delay(delay, ct);
        }
    }
}