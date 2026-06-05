using Application.Dtos.Products.BotSide;
using Domain.Models.Products;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<Result<List<BotSideUserWatchListProductsDto>>> GetUserProducts(long chatId);

        Task<Result<BotSideProductDto>> CreateProductForUser(BotSideCreateProductForUserDto dto);

        Task<Result> RemoveProductFromUserAsync(BotSideProductUserInfoDto dto);

        Task<Result<BotSideProductDto>> GetProductByDigiKalaId(string digiKalaId);

        Task<Result<BotSideProductDto>> GetProductForUserByDigiKalaId(BotSideGetProductForUserDto dto);

        Task<Result> UpdateProductPrices(BotSideUpdateProductPricesDto dto);

        Task<List<Product>> GetProductsBatchForMonitoringAsync(BotSideGetProductsBatchForMonitoringDto dto);
    }
}
