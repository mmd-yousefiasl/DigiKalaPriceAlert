namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class UpdateSellingPriceData
    {
        public int? VariantId { get; init; }
        public int? SellingPrice { get; init; }
        public int? CreditIncreasePercentage { get; init; }
    }
}
