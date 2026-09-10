using Application.Dtos.DkMarketplace.Common;

namespace Application.Dtos.DkMarketplace.Auth
{
    public sealed class ScopeListData
    {
        public DkSortData? SortData { get; init; }
        public DkPager? Pager { get; init; }
        public List<object>? FormData { get; init; }
        public List<ScopeItem>? Items { get; init; }
        public Dictionary<string, object>? MetaData { get; init; }
    }
}
