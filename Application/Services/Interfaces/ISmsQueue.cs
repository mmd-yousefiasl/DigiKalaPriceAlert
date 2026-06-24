using Application.Dtos.PriceMonitor;

namespace Application.Services.Interfaces
{
    public interface ISmsQueue
    {
        ValueTask QueueSmsAsync(SmsMessageDto message);
        ValueTask<SmsMessageDto> DequeueSmsAsync(CancellationToken cancellationToken);
    }
}
