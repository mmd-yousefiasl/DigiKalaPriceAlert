namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class VariantsQuery
    {
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? Sort { get; init; }
        public string? Order { get; init; }
        public int? Id { get; init; }
        public string? Ids { get; init; }
        public string? ShippingType { get; init; }
        public bool? Active { get; init; }
        public string? ModerationStatus { get; init; }
        public string? CategoryIds { get; init; }
        public string? BuyBoxWinner { get; init; }
        public bool? InCompetition { get; init; }
        public string? SearchTerm { get; init; }
        public string? PriceTerms { get; init; }
        public bool? OutOfStock { get; init; }
        public bool? Archived { get; init; }
        public string? SellingChannel { get; init; }
        public string? CreationTimeFrom { get; init; }
        public string? CreationTimeTo { get; init; }
        public string? SellerProductTags { get; init; }
        public bool? NearbySellerShipment { get; init; }
    }
}
