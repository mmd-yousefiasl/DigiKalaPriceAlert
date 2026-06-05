namespace Application.Dtos.Products.BotSide
{
    public class BotSidePriceChangedProductDto
    {
        public string Title { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string DigiKalaId { get; set; }
        public string? ImageFileName { get; set; }
    }
}
