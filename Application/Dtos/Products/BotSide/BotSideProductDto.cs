namespace Application.Dtos.Products.BotSide
{
    public class BotSideProductDto : CommonDto
    {
        public string DigiKalaId { get; set; }
        public string Title { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public string Url { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string? ImageFileName { get; set; }
    }
}
