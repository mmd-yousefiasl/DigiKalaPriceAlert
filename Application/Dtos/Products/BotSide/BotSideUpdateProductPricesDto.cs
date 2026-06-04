namespace Application.Dtos.Products.BotSide
{
    public class BotSideUpdateProductPricesDto
    {
        public int ProductId { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
