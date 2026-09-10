namespace Application.Dtos.DkMarketplace.Common
{
    internal sealed class DkErrorEnvelope
    {
        public string? Status { get; init; }
        public int? Code { get; init; }
        public string? Message { get; init; }
        public Dictionary<string, string[]>? Errors { get; init; }
    }
}
