namespace Application.Dtos.TelegramDto
{
    public class SendTextMessageDto
    {
        public long ChatId { get; set; }
        public string Message { get; set; }
    }
}
