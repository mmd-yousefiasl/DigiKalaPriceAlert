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

        private const int BatchSize = 50;
        private const int CycleIntervalMinutes = 2;

        private int _requestCount;
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

            var cycleInterval = TimeSpan.FromMinutes(CycleIntervalMinutes);

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
            var userNotifications = new Dictionary<int, ClientSideUserChangeNotification>();

            _requestCount = 0;
            int page = 0;
            int totalProcessed = 0;
            bool shouldAbortCycle = false;

            while (!ct.IsCancellationRequested && !shouldAbortCycle)
            {
                // ── Load one batch ───────────────────────────────────────
                List<ProductMonitoringTargetDto> batch;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                    var result = await productService.GetProductBatchForMonitoringAsync(page, BatchSize, ct);

                    if (result.IsFailure || !result.Value.Any())
                        break;

                    batch = result.Value;
                }

                _logger.LogInformation(
                    "Processing batch {Page} ({Count} products, total so far: {Total})",
                    page + 1,
                    batch.Count,
                    totalProcessed);

                // ── Process each product in the batch ────────────────────
                foreach (var product in batch)
                {
                    if (ct.IsCancellationRequested || shouldAbortCycle)
                        break;

                    try
                    {
                        await ThrottleAsync(ct);

                        using var productScope = _scopeFactory.CreateScope();

                        var scraperService = productScope.ServiceProvider.GetRequiredService<IDigiKalaService>();
                        var productService = productScope.ServiceProvider.GetRequiredService<IProductService>();

                        // Scrape
                        var scrapeResult = await scraperService.ScrapeFullProductAsync(product.DigiKalaId, ct);

                        if (scrapeResult.IsFailure)
                        {
                            if (!string.IsNullOrWhiteSpace(scrapeResult.Message) &&
                                scrapeResult.Message.Contains("RATE_LIMITED"))
                            {
                                _logger.LogCritical(
                                    "Rate limit detected for DigiKalaId: {DigiKalaId}. Aborting cycle.",
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

                        // Sync + detect changes
                        var syncResult = await productService.SyncProductAndDetectChangesAsync(
                            product,
                            scrapeResult.Value,
                            ct);

                        if (syncResult.IsFailure)
                        {
                            _logger.LogWarning(
                                "Sync failed for DigiKalaId: {DigiKalaId}. Error: {Error}",
                                product.DigiKalaId,
                                syncResult.Message);

                            continue;
                        }

                        // Merge notifications
                        foreach (var notification in syncResult.Value)
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

                        totalProcessed++;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error processing ProductId: {ProductId}, DigiKalaId: {DigiKalaId}",
                            product.ProductId,
                            product.DigiKalaId);

                        continue;
                    }
                }

                // If batch was smaller than BatchSize, we've reached the end
                if (batch.Count < BatchSize)
                    break;

                page++;
            }

            _logger.LogInformation("Processed {Total} products total in this cycle.", totalProcessed);

            // ── Send SMS notifications ────────────────────────────────────
            if (ct.IsCancellationRequested || userNotifications.Count == 0)
            {
                _logger.LogInformation("No users need notification in this cycle.");
                return;
            }

            _logger.LogInformation("Sending SMS to {Count} unique users.", userNotifications.Count);

            await SendNotificationsAsync(userNotifications, ct);
        }

        private async Task SendNotificationsAsync(
            Dictionary<int, ClientSideUserChangeNotification> userNotifications,
            CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();

            var smsService = scope.ServiceProvider.GetRequiredService<ISendSmsService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var userContacts = await userService.GetUserContactsAsync(
                userNotifications.Keys.ToList(), ct);

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