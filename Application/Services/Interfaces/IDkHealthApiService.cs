using Application.Dtos.DkMarketplace.Health;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IDkHealthApiService
    {
        Task<Result<HealthCheckResponse>> GetServiceDetailsAsync(CancellationToken ct = default);
    }
}
