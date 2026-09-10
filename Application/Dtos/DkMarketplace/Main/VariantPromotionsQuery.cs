namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class VariantPromotionsQuery
    {
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? Sort { get; init; }
        public string? Order { get; init; }
    }
}
