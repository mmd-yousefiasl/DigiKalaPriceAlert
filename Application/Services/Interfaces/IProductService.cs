using Application.Dtos.Products.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<Result<ClientSideProductDetailsDto>> ClientSideGetProductDetails(int id);

        Task<Result<ClientSideFilterProductsDto>> ClientSideFilterProducts(ClientSideFilterProductsDto filter);

        Task<Result> ClientSideDeleteUserProduct(int id);

        Task<Result<List<ProductMonitoringTargetDto>>> GetProductBatchForMonitoringAsync(int page, int batchSize, CancellationToken ct);

        Task<Result> ClientSideSetProductAsReadForUser(int id);

        Task<Result<List<ClientSideUserChangeNotification>>> SyncProductAndDetectChangesAsync(ProductMonitoringTargetDto target, ClientSideScrapedProductDto scraped, CancellationToken ct);

        Task<Result<ClientSideProductSearchResultDto>> SearchProductForModalAsync(string url);

        Task<Result> AddProductToUserTrackingAsync(ClientSideAddTrackingDto dto);

        Task ExecuteUpdateCycleAsync(CancellationToken ct = default);
    }
}