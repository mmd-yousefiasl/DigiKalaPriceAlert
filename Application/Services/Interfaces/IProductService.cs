using Application.Dtos.Products.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<Result<ClientSideProductDetailsDto>> ClientSideGetProductDetails(int id);

        Task<Result<ClientSideFilterProductsDto>> ClientSideFilterProducts(ClientSideFilterProductsDto filter);

        Task<Result> ClientSideDeleteUserProduct(int id);

        Task<Result> ClientSideSetProductAsReadForUser(int id);

        Task<Result<ClientSideProductSearchResultDto>> SearchProductForModalAsync(string url);

        Task<Result> AddProductToUserTrackingAsync(ClientSideAddTrackingDto dto);

        Task ExecuteUpdateCycleAsync(CancellationToken ct = default);
    }
}