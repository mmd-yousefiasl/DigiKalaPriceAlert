namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class UpdateSellingPriceRequest
    {
        public required int VariantId { get; init; }
        public required int SellingPrice { get; init; }
        public required int CreditIncreasePercentage { get; init; }
    }
}
