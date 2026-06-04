using Application.Dtos.TelegramDto;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Services.Interfaces
{
    public interface ITelegramService
    {
        Task SendProductAlertAsync(SendProductAlertDto dto);

        Task SendTextMessageAsync(SendTextMessageDto dto);

        Task EditMessageReplyMarkupAsync(long chatId, int messageId, InlineKeyboardMarkup replyMarkup);

        Task EditMessageTextAsync(long chatId, int messageId, string newText);

        Task AnswerCallbackQueryAsync(string callbackQueryId, string text);

        Task SendMainMenuAsync(long chatId, string text);

        Task SendShareContactAsync(long chatId, string text);

        Task SendTextMessageWithMarkupAsync(SendTextMessageWithMarkupDto dto);

        Task DeleteMessageAsync(long chatId, int messageId);
    }
}
