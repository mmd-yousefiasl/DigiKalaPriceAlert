namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class SellerProductsQuery
    {
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? Sort { get; init; }
        public string? Order { get; init; }
        public string? MultiSearch { get; init; }
        public string? CategoryId { get; init; }
        public string? BrandId { get; init; }
        public string? ModerationStatus { get; init; }
        public string? FakeStatus { get; init; }
    }
}
