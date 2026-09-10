namespace Application.Dtos.DkMarketplace.Auth
{
    public sealed class CreateTokenRequest
    {
        public required string AuthorizationCode { get; init; }
    }
}
