namespace Application.Services.Interfaces
{
    public interface ITelegramUpdateQueue
    {
        ValueTask EnqueueAsync(string body, CancellationToken cancellationToken = default);

        ValueTask<string> DequeueAsync(CancellationToken cancellationToken);
    }
}
