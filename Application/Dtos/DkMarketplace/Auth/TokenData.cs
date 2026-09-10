namespace Application.Dtos.DkMarketplace.Auth
{
    public sealed class TokenData
    {
        public string? AccessToken { get; init; }
        public string? RefreshToken { get; init; }
        public DkDateTimeInfo? AccessTokenExpiresAt { get; init; }
        public DkDateTimeInfo? RefreshTokenExpiresAt { get; init; }
    }
}
