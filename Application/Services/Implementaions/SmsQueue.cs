using Application.Dtos.NotificationQueues;
using Application.Services.Interfaces;
using System.Threading.Channels;

namespace Application.Services.Implementaions
{
    public class SmsQueue : ISmsQueue
    {
        #region Fields
        private readonly Channel<SmsMessage> _queue;
        #endregion

        #region Ctor
        public SmsQueue()
        {
            _queue = Channel.CreateUnbounded<SmsMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }
        #endregion

        public async ValueTask QueueSmsAsync(SmsMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);
            await _queue.Writer.WriteAsync(message);
        }

        public async ValueTask<SmsMessage> DequeueSmsAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
