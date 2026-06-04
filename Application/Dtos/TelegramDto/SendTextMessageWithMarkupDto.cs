using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Dtos.TelegramDto
{
    public class SendTextMessageWithMarkupDto
    {
        public long ChatId { get; set; }
        public string Text { get; set; }
        public InlineKeyboardMarkup ReplyMarkup { get; set; }
    }
}
