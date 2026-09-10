namespace Application.Dtos.DkMarketplace.Common
{
    public sealed class DkPager
    {
        public int? Page { get; init; }
        public int? ItemPerPage { get; init; }
        public int? TotalPages { get; init; }
        public int? TotalRows { get; init; }
    }
}
