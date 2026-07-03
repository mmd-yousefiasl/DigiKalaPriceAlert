using Application.Dtos.DigiKala;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IDigiKalaService
    {
        Result<string> ExtractProductIdFromUrl(string url);

        Task<Result<DigiKalaProductResultDto>> GetProductDetails(string digiKalaId);

        Task<Result<DigiKalaProductResultDto>> GetCurrentPublicIpAsync();
    }
}
