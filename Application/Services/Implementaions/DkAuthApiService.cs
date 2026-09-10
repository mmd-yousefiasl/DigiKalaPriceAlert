using Application.Dtos.DkMarketplace.Auth;
using Application.Dtos.DkMarketplace.Common;
using Application.Services.Interfaces;
using Domain.Shared;
using Domain.Statics;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementaions
{
    public sealed class DkAuthApiService : DkApiServiceBase, IDkAuthApiService
    {
        #region Ctor
        public DkAuthApiService(HttpClient httpClient, ILogger<DkAuthApiService> logger)
            : base(httpClient, logger) { }
        #endregion

        #region Methods
        public Task<Result<ScopeListData>> GetScopesAsync(CancellationToken ct = default)
            => GetAsync<ScopeListData>($"{SiteTools.DkApiServiceBase}/auth/scopes", ct);

        public Task<Result<ScopeListData>> GetClientScopesAsync(string clientCode, CancellationToken ct = default)
            => GetAsync<ScopeListData>($"{SiteTools.DkApiServiceBase}/auth/scopes/{Uri.EscapeDataString(clientCode)}", ct);

        public Task<Result<TokenData>> CreateTokenAsync(CreateTokenRequest request, CancellationToken ct = default)
            => PostAsync<TokenData>($"{SiteTools.DkApiServiceBase}/auth/token", request, ct);

        public Task<Result<TokenData>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
            => PostAsync<TokenData>($"{SiteTools.DkApiServiceBase}/auth/refresh-token", request, ct);

        public Task<Result<RevokeTokenData>> RevokeTokenAsync(CancellationToken ct = default)
            => PostAsync<RevokeTokenData>($"{SiteTools.DkApiServiceBase}/auth/revoke", body: null, ct);
        #endregion
    }
}