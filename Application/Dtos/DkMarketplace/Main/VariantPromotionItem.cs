using Application.Dtos.DkMarketplace.Auth;

namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class VariantPromotionItem
    {
        public int? PromotionId { get; init; }
        public string? VariantStatus { get; init; }
        public string? PromotionTitle { get; init; }
        public string? PromotionType { get; init; }
        public DkDateTimeInfo? StartAt { get; init; }
        public DkDateTimeInfo? EndAt { get; init; }
        public int? PromotionRrpPrice { get; init; }
        public int? PromotionSellingPrice { get; init; }
    }
}
