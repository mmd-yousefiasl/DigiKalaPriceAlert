using Application.Dtos.NotificationQueues;
using Application.Dtos.Products.BotSide;
using Application.Dtos.Users.BotSide;
using Application.Services.Interfaces;
using Application.Tools;

namespace API.BackgroundServices
{
    public class PriceMonitorWorker : BackgroundService
    {
        #region Fields
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly INotificationQueue _notificationQueue;
        private readonly ILogger<PriceMonitorWorker> _logger;
        #endregion

        #region Ctor
        public PriceMonitorWorker(
            IServiceScopeFactory scopeFactory,
            INotificationQueue notificationQueue,
            ILogger<PriceMonitorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _notificationQueue = notificationQueue;
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
            int page = 0;
            bool hasMoreProducts = true;

            while (hasMoreProducts && !stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                var scraperService = scope.ServiceProvider.GetRequiredService<IDigiKalaService>();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                var productsBatch = await productService.GetProductsBatchForMonitoringAsync(new BotSideGetProductsBatchForMonitoringDto()
                {
                    Page = page,
                    BarchSize = AppInfo.PriceMonitorBatchSize,
                    CancellationToken = stoppingToken
                });

                if (!productsBatch.Any())
                {
                    hasMoreProducts = false;
                    break;
                }

                foreach (var product in productsBatch)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var scrapResult = await scraperService.GetProductDetailsWithoutImageAsync(product.DigiKalaId);
                    if (scrapResult.IsFailure) continue;

                    decimal newPrice = scrapResult.Value.Price;
                    decimal oldPrice = product.NewPrice;

                    if (newPrice != oldPrice)
                    {
                        await productService.UpdateProductPrices(new BotSideUpdateProductPricesDto()
                        {
                            ProductId = product.Id,
                            OldPrice = oldPrice,
                            NewPrice = newPrice,
                            CancellationToken = stoppingToken
                        });

                        var trackingUsers = await userService.BotSideGetUserChatIdsTrackingThisProduct(new BotSideGetUsersTrackingProductDto()
                        {
                            ProductId = product.Id,
                            CancellationToken = stoppingToken
                        });

                        foreach (var chatId in trackingUsers)
                        {
                            await _notificationQueue.QueueNotificationAsync(new NotificationMessage(
                                ChatId: chatId,
                                Title: product.Title,
                                OldPrice: oldPrice,
                                NewPrice: newPrice,
                                DigiKalaId: product.DigiKalaId,
                                ImageFileName: product.ImageFileName
                            ));
                        }
                    }
                }

                page++;
            }
        }
    }
}
