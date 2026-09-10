using Application.Dtos.DkMarketplace.Common;
using Application.Dtos.DkMarketplace.Health;
using Application.Services.Interfaces;
using Domain.Shared;
using Domain.Statics;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementaions
{
    public sealed class DkHealthApiService : DkApiServiceBase, IDkHealthApiService
    {
        #region Ctor
        public DkHealthApiService(HttpClient httpClient, ILogger<DkHealthApiService> logger)
            : base(httpClient, logger) { }
        #endregion

        #region Methods
        public Task<Result<HealthCheckResponse>> GetServiceDetailsAsync(CancellationToken ct = default)
            => GetAsync<HealthCheckResponse>(SiteTools.DkApiServiceBase, ct);
        #endregion
    }
}