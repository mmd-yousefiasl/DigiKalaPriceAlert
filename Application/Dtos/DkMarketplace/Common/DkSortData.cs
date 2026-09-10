namespace Application.Dtos.DkMarketplace.Common
{
    public sealed class DkSortData
    {
        public string? SortColumn { get; init; }
        public string? SortOrder { get; init; }
        public List<string>? SortColumns { get; init; }
    }
}
