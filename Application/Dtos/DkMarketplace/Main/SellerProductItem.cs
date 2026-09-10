namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class SellerProductItem
    {
        public int? VariantsCount { get; init; }
        public string? Site { get; init; }
        public string? Title { get; init; }
        public string? Status { get; init; }
        public int? ProductId { get; init; }
        public bool? Fake { get; init; }
        public string? StatusData { get; init; }
        public bool? IsOwner { get; init; }
        public Dictionary<string, string>? MainCategoryTitle { get; init; }
        public bool? Active { get; init; }
        public string? TitleFa { get; init; }
        public string? TitleEn { get; init; }
        public int? BrandId { get; init; }
        public string? BrandTitleEn { get; init; }
        public string? BrandTitleFa { get; init; }
        public string? BrandTitle { get; init; }
        public string? ProductUrl { get; init; }
        public string? ImageSrc { get; init; }
        public string? DimensionLevel { get; init; }
        public ModerationStatusInfo? ModerationStatus { get; init; }
        public string? AdvergeUrl { get; init; }
        public ProductDimension? ProductDimension { get; init; }
    }
}
