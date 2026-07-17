using Application.Dtos.Products.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<Result<int>> ClientSideCreateNewProduct(ClientSideCreateNewProductDto dto);

        Task<Result<ClientSideProductDetailsDto>> ClientSideGetProductDetails(int id);

        Task<Result<ClientSideFilterProductsDto>> ClientSideFilterProducts(ClientSideFilterProductsDto filter);

        Task<Result> ClientSideDeleteUserProduct(int id);

        Task<List<ProductMonitoringTargetDto>> GetAllProductsForMonitoringAsync(CancellationToken ct);

        Task<Result> ClientSideSetProductAsReadForUser(int id);

        Task<List<ClientSideUserChangeNotification>> SyncProductAndDetectChangesAsync(ProductMonitoringTargetDto target, ClientSideScrapedProductDto scraped, CancellationToken ct);
    }
}