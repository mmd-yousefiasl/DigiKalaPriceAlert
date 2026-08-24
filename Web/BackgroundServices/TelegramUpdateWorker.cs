using Application.Services.Interfaces;

namespace Web.BackgroundServices
{
    public class TelegramUpdateWorker : BackgroundService
    {
        private readonly ITelegramUpdateQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TelegramUpdateWorker> _logger;

        public TelegramUpdateWorker(
            ITelegramUpdateQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<TelegramUpdateWorker> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var body = await _queue.DequeueAsync(stoppingToken);

                    using var scope = _scopeFactory.CreateScope();

                    var telegramService = scope.ServiceProvider
                        .GetRequiredService<ITelegramService>();

                    await telegramService.ProcessTelegramUpdateAsync(body);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while processing Telegram update.");
                }
            }
        }
    }
}
