using Telegram.Bot.Types;

namespace Application.Services.Interfaces
{
    public interface ITelegramUpdateHandler
    {
        Task HandleUpdateAsync(Update update);
    }
}
