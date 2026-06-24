using Application.Dtos.PriceMonitor;
using Application.Services.Interfaces;
using System.Threading.Channels;

namespace Application.Services.Implementaions
{
    public class SmsQueue : ISmsQueue
    {
        #region Fields
        private readonly Channel<SmsMessageDto> _channel;
        #endregion

        #region Ctor
        public SmsQueue()
        {
            var options = new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            };

            _channel = Channel.CreateUnbounded<SmsMessageDto>(options);
        }
        #endregion

        public ValueTask QueueSmsAsync(SmsMessageDto message)
        {
            return _channel.Writer.WriteAsync(message);
        }

        public ValueTask<SmsMessageDto> DequeueSmsAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
