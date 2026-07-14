using Application.Dtos.DigiKala;
using Application.Dtos.Products.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IDigiKalaService
    {
        Result<string> ExtractProductIdFromUrl(string url);

        Task<Result<DigiKalaProductResultDto>> GetProductDetails(string digiKalaId);

        Task<Result<ClientSideScrapedProductDto>> ScrapeFullProductAsync(string digiKalaId, CancellationToken ct = default);
    }
}
