using Application.Dtos.DkMarketplace.Auth;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IDkAuthApiService
    {
        Task<Result<ScopeListData>> GetScopesAsync(CancellationToken ct = default);
        Task<Result<ScopeListData>> GetClientScopesAsync(string clientCode, CancellationToken ct = default);
        Task<Result<TokenData>> CreateTokenAsync(CreateTokenRequest request, CancellationToken ct = default);
        Task<Result<TokenData>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
        Task<Result<RevokeTokenData>> RevokeTokenAsync(CancellationToken ct = default);
    }
}
