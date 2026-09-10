namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class SellingPriceBoundaryData
    {
        public PriceBoundary? DefaultPriceBoundary { get; init; }
        public PriceBoundary? PromotionPriceBoundary { get; init; }
    }
}
