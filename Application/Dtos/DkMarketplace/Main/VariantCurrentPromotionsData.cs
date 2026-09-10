using Application.Dtos.DkMarketplace.Common;

namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class VariantCurrentPromotionsData
    {
        public DkSortData? SortData { get; init; }
        public DkPager? Pager { get; init; }
        public List<object>? FormData { get; init; }
        public List<VariantPromotionItem>? Items { get; init; }
        public Dictionary<string, object>? MetaData { get; init; }
    }
}
