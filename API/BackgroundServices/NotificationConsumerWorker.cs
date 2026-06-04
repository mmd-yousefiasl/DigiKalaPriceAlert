using Application.Dtos.TelegramDto;
using Application.Services.Interfaces;
using Application.Tools;

namespace API.BackgroundServices
{
    public class NotificationConsumerWorker : BackgroundService
    {
        #region Fields
        private readonly INotificationQueue _queue;
        private readonly ITelegramService _telegramService;
        private readonly ILogger<NotificationConsumerWorker> _logger;
        #endregion

        #region Ctor
        public NotificationConsumerWorker(
            INotificationQueue queue,
            ITelegramService telegramService,
            ILogger<NotificationConsumerWorker> logger)
        {
            _queue = queue;
            _telegramService = telegramService;
            _logger = logger;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Consumer Worker started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var notification = await _queue.DequeueNotificationAsync(stoppingToken);

                    await _telegramService.SendProductAlertAsync(new SendProductAlertDto()
                    {
                        ChatId = notification.ChatId,
                        Title = notification.Title,
                        OldPrice = notification.OldPrice,
                        NewPrice = notification.NewPrice,
                        DigiKalaId = notification.DigiKalaId,
                        ImageFileName = notification.ImageFileName,
                        ImagePath = AppInfo.ImageCacheFolder
                    });

                    await Task.Delay(50, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending queued notification.");
                }
            }
        }
    }
}
