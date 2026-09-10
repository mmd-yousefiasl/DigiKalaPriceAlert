namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class BatchInquiryData
    {
        public int? BatchId { get; init; }
        public string? Status { get; init; }
        public List<BatchInquiryItem>? Items { get; init; }
    }
}
