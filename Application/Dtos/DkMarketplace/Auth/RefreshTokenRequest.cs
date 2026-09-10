namespace Application.Dtos.DkMarketplace.Auth
{
    public sealed class RefreshTokenRequest
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
    }
}
