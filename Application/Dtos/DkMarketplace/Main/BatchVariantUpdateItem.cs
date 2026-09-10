namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class BatchVariantUpdateItem
    {
        public required int VariantId { get; init; }
        public BatchVariantUpdatePayload? Payload { get; init; }
    }
}
