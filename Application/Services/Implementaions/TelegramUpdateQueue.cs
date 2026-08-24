using Application.Services.Interfaces;
using System.Threading.Channels;

namespace Application.Services.Implementaions
{
    public class TelegramUpdateQueue : ITelegramUpdateQueue
    {
        private readonly Channel<string> _queue;

        public TelegramUpdateQueue()
        {
            _queue = Channel.CreateUnbounded<string>();
        }

        public async ValueTask EnqueueAsync(
            string body,
            CancellationToken cancellationToken = default)
        {
            await _queue.Writer.WriteAsync(body, cancellationToken);
        }

        public async ValueTask<string> DequeueAsync(
            CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
