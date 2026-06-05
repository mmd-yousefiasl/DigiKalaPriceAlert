using Application.Services.Interfaces;

namespace API.BackgroundServices
{
    public class SmsConsumerWorker : BackgroundService
    {
        #region Fields
        private readonly ISmsQueue _smsQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SmsConsumerWorker> _logger;
        #endregion

        #region Ctor
        public SmsConsumerWorker(
        ISmsQueue smsQueue,
        IServiceProvider serviceProvider,
        ILogger<SmsConsumerWorker> logger)
        {
            _smsQueue = smsQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sms Consumer Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // منتظر می‌ماند تا یک پیامک وارد صف شود
                    var smsMessage = await _smsQueue.DequeueSmsAsync(stoppingToken);

                    // ایجاد Scope برای استفاده از سرویس‌های Scoped مثل SmsService
                    using var scope = _serviceProvider.CreateScope();
                    //   var smsService = scope.ServiceProvider.GetRequiredService<ISmsService>();

                    // ارسال اس‌ام‌اس واقعی از طریق پنل شما
                    //   await smsService.SendSmsAsync(smsMessage.Mobile, smsMessage.Text);
                }
                catch (OperationCanceledException)
                {
                    // برنامه در حال بسته شدن است
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending SMS from queue.");
                    // یک تاخیر کوتاه برای اینکه در صورت خرابی پنل، سرور در لوپ بی‌نهایت ارور نیفتد
                    await Task.Delay(2000, stoppingToken);
                }
            }
        }
    }
}
