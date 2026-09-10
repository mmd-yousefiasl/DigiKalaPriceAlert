namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class CommissionItem
    {
        public int? MainCategoryId { get; init; }
        public string? MainCategoryTitle { get; init; }
        public int? CategoryId { get; init; }
        public string? CategoryTitle { get; init; }
        public int? BrandId { get; init; }
        public string? BrandTitle { get; init; }
        public double? Commission { get; init; }
        public int? CurrentLeadTime { get; init; }
        public double? ExtraCommission { get; init; }
        public double? ReturnFreeCommission { get; init; }
    }
}
