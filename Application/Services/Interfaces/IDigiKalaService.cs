using Application.Dtos.DigiKala;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IDigiKalaService
    {
        Result<string> ExtractProductIdFromUrl(string url);

        Task<Result<DigiKalaProductResultDto>> GetProductDetailsAsync(string digiKalaId);

        Task<Result<DigiKalaProductResultDto>> GetProductDetailsWithoutImageAsync(string digiKalaId);
    }
}
