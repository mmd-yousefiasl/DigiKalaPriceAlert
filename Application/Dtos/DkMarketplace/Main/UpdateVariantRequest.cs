namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class UpdateVariantRequest
    {
        public int? SellerStock { get; init; }
        public int? MaximumPerOrder { get; init; }
        public int? SellingPrice { get; init; }
        public int? PriceLimit { get; init; }
        public double? CreditIncreasePercentage { get; init; }
        public string? ShippingType { get; init; }
        public int? LeadTime { get; init; }
        public int? SellerLeadTime { get; init; }
        public bool? Activation { get; init; }
        public bool? ThreeHourDelivery { get; init; }
    }
}
