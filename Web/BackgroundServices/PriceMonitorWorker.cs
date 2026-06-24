using Application.Dtos.PriceMonitor;
using Application.Dtos.Products.ClientSide;
using Application.Dtos.Users.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Domain.Shared;

namespace Web.BackgroundServices
{
    public class PriceMonitorWorker : BackgroundService
    {
        #region Fields
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PriceMonitorWorker> _logger;
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
            _logger.LogInformation("Price Monitor Worker Started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting a new price monitoring cycle at: {Time}", DateTimeOffset.Now);
                    await RunMonitoringCycleAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the price monitoring cycle.");
                }

                _logger.LogInformation("Monitoring cycle finished. Sleeping for 3 hours...");
                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
        }

        private async Task RunMonitoringCycleAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var scraperService = scope.ServiceProvider.GetRequiredService<IDigiKalaService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var productIdsResult = await userService.ClientSideGetAllEligibleProductIdsAsync(stoppingToken);
            if (productIdsResult.IsFailure || !productIdsResult.Value.Any())
                return;

            var allProductIds = productIdsResult.Value;
            int batchSize = AppInfo.PriceMonitorBatchSize;
            int page = 0;
            bool hasMoreProducts = true;

            var mobilesToNotify = new List<string>();

            while (hasMoreProducts && !stoppingToken.IsCancellationRequested)
            {
                var batchIds = allProductIds.Skip(page * batchSize).Take(batchSize).ToList();

                if (!batchIds.Any())
                {
                    hasMoreProducts = false;
                    break;
                }

                var productsBatch = await productService.GetProductsBatchForMonitoringAsync(new ClientSideGetProductsBatchForMonitoringDto()
                {
                    ProductIds = batchIds,
                    CancellationToken = stoppingToken
                });

                if (!productsBatch.Any())
                {
                    page++;
                    continue;
                }
                var random = new Random();

                foreach (var product in productsBatch)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    int delay = random.Next(1000, 2800);
                    await Task.Delay(delay, stoppingToken);

                    var scrapResult = await scraperService.GetProductDetailsWithoutImage(product.DigiKalaId);
                    if (scrapResult.IsFailure) continue;

                    decimal newPrice = scrapResult.Value.Price;
                    decimal oldPrice = product.NewPrice;

                    if (newPrice != oldPrice)
                    {
                        await productService.ClientSideUpdateProductPrices(new ClientSideUpdateProductPricesDto()
                        {
                            ProductId = product.Id,
                            OldPrice = oldPrice,
                            NewPrice = newPrice,
                            CancellationToken = stoppingToken
                        });

                        var userMobiles = await userService.ClientSideGetUsersTrackingProduct(new ClientSideGetUsersTrackingProductDto()
                        {
                            ProductId = product.Id,
                            CancellationToken = stoppingToken
                        });

                        foreach (var item in userMobiles)
                        {
                            mobilesToNotify.Add(item);
                        }
                    }
                }

                page++;
            }

            var uniqueMobiles = mobilesToNotify.Distinct().ToList();

            if (uniqueMobiles.Any() && !stoppingToken.IsCancellationRequested)
            {
                var smsQueue = scope.ServiceProvider.GetRequiredService<ISmsQueue>();
                string textMessage = Messages.ProductPriceChangedMessage;

                foreach (var mobile in uniqueMobiles)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    await smsQueue.QueueSmsAsync(new SmsMessageDto(mobile, textMessage));
                }
            }
        }
    }
}
