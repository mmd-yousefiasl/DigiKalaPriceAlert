namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class BatchVariantUpdateRequest
    {
        public int? Deadline { get; init; }
        public required List<BatchVariantUpdateItem> Items { get; init; }
    }
}
