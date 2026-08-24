using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ITelegramService
    {
        Task ProcessTelegramUpdateAsync(string body);

        Task<Result> SendTelegramMessage(long chatId, string messageText);

        Task<Result> SendTelegramTestMessage();
    }
}
