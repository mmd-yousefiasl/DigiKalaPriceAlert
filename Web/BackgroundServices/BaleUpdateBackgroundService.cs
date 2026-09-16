using Application.Services.Interfaces;

namespace Web.BackgroundServices
{
    public class BaleUpdateBackgroundService : BackgroundService
    {
        private readonly IBaleUpdateQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BaleUpdateBackgroundService> _logger;

        public BaleUpdateBackgroundService(
            IBaleUpdateQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<BaleUpdateBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bale update consumer started");

            await foreach (var update in _queue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    // Fresh scope per update so scoped services (DbContext, etc.) are safe
                    using var scope = _scopeFactory.CreateScope();
                    var baleService = scope.ServiceProvider.GetRequiredService<IBaleService>();

                    await baleService.HandleUpdateAsync(update, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Swallow so one bad update doesn't kill the consumer
                    _logger.LogError(ex, "Failed to handle Bale update {UpdateId}", update.UpdateId);
                }
            }

            _logger.LogInformation("Bale update consumer stopped");
        }
    }
}
