namespace Application.Dtos.DkMarketplace.Auth
{
    public sealed class ScopeItem
    {
        public string? Key { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
        public string? Access { get; init; } // "write" | "read"
    }
}
