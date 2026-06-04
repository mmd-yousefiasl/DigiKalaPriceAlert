namespace Application.Dtos.TelegramDto
{
    public class SendProductAlertDto
    {
        public long ChatId { get; set; }
        public string Title { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public string DigiKalaId { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImagePath { get; set; }
    }
}
