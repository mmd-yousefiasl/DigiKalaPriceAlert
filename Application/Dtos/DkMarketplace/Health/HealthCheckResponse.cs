namespace Application.Dtos.DkMarketplace.Health
{
    public sealed class HealthCheckResponse
    {
        public string? Status { get; init; }
        public Dictionary<string, object>? Details { get; init; }
    }
}
