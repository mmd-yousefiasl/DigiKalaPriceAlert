namespace Application.Dtos.Users.BotSide
{
    public class BotSideUserProductDetailsDto
    {
        public int ProductId { get; set; }
        public string DigiKalaId { get; set; }
        public string Title { get; set; }
        public decimal CurrentPrice { get; set; }
        public string? ImageFileName { get; set; }
    }
}
