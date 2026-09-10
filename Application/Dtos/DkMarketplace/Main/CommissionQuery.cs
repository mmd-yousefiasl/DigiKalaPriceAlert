namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class CommissionQuery
    {
        public int? Page { get; init; }
        public int? Size { get; init; }
        public string? Sort { get; init; }
        public string? Order { get; init; }
        public bool? MyCommissions { get; init; }
        public int? MainCategoryId { get; init; }
        public int? CategoryId { get; init; }
        public int? BrandId { get; init; }
    }
}
