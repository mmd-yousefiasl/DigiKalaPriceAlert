using Application.Dtos.DigiKala;
using Application.Dtos.Products.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IDigiKalaProductParser
    {
        Result<ClientSideScrapedProductDto> Parse(string digiKalaId, DigiKalaApiResponse response);
    }
}
