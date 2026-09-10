using Application.Dtos.DkMarketplace.Common;

namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class VariantListData
    {
        public DkSortData? SortData { get; init; }
        public DkPager? Pager { get; init; }
        public List<object>? FormData { get; init; }
        public List<VariantItem>? Items { get; init; }
        public Dictionary<string, object>? MetaData { get; init; }
    }
}
