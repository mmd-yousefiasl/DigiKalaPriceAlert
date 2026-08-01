using Application.Dtos.DigiKala;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IDigiKalaProductApiClient
    {
        Task<Result<DigiKalaApiResponse>> GetProductAsync(string digiKalaId, CancellationToken ct);
    }
}
