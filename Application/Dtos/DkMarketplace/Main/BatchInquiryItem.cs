namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class BatchInquiryItem
    {
        public int? Key { get; init; }
        public string? Status { get; init; }
        public Dictionary<string, object>? Errors { get; init; }
    }
}
