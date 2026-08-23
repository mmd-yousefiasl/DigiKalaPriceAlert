namespace Application.Dtos.Telegram
{
    public class TelegramMessageDto
    {
        public TelegramChatDto? Chat { get; set; }
        public string? Text { get; set; }
    }
}
