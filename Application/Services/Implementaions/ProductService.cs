using Application.Dtos.Products.BotSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Products;
using Domain.Models.Users;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementaions
{
    public class ProductService : IProductService
    {
        #region Fields
        private readonly IDigiKalaService _digiKalaService;
        private readonly IUserService _userService;
        private readonly DigiKalaPriceAlertDbContext _context;
        private readonly ILogger<ProductService> _logger;
        #endregion

        #region Ctor
        public ProductService(
            IDigiKalaService digiKalaService,
            ILogger<ProductService> logger,
            DigiKalaPriceAlertDbContext context,
            IUserService userService)
        {
            _digiKalaService = digiKalaService;
            _context = context;
            _logger = logger;
            _userService = userService;
        }
        #endregion

        public async Task<Result> RemoveProductFromUserAsync(BotSideProductUserInfoDto dto)
        {
            var userProduct = await _context.UserProductMapping
                .FirstOrDefaultAsync(up => up.User.ChatId == dto.ChatId && up.Product.DigiKalaId == dto.DigiKalaId);

            if (userProduct is null)
                return Result.Failure(BotMessages.ProductNotFoundInServerError);

            _context.UserProductMapping.Remove(userProduct);
            await _context.SaveChangesAsync();

            return Result.Success(BotMessages.ProductDeletedSuccessfullyMessage);
        }

        public async Task<Result<BotSideProductDto>> GetProductByDigiKalaId(string digiKalaId)
        {
            var product = await _context.Products
                        .AsNoTracking()
                        .Where(p => p.DigiKalaId == digiKalaId)
                        .Select(p => new BotSideProductDto()
                        {
                            Id = p.Id,
                            Title = p.Title,
                            DigiKalaId = p.DigiKalaId,
                            ImageFileName = p.ImageFileName,
                            NewPrice = p.NewPrice,
                            OldPrice = p.OldPrice,
                            LastUpdatedDate = p.LastUpdatedDate,
                            Url = p.Url
                        }).FirstOrDefaultAsync();

            if (product == null)
                return Result.Failure<BotSideProductDto>(BotMessages.YouDontCheckThisProductError);

            return product;
        }

        public async Task<Result> UpdateProductPrices(BotSideUpdateProductPricesDto dto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(dto.CancellationToken);
                try
                {
                    var rowsAffected = await _context.Products
                        .Where(p => p.Id == dto.ProductId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.OldPrice, p => p.NewPrice)
                            .SetProperty(p => p.NewPrice, dto.NewPrice)
                            .SetProperty(p => p.LastUpdatedDate, DateTime.UtcNow),
                        dto.CancellationToken);

                    if (rowsAffected == 0)
                        return Result.Failure(Messages.OperationFailedError);

                    var history = new ProductPriceHistory
                    {
                        ProductId = dto.ProductId,
                        Price = dto.NewPrice,
                    };
                    _context.ProductPriceHistory.Add(history);

                    await _context.SaveChangesAsync(dto.CancellationToken);

                    await transaction.CommitAsync(dto.CancellationToken);

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(dto.CancellationToken);
                    _logger.LogError(ex, "Error updating prices for product {ProductId}", dto.ProductId);
                    throw;
                }
            });
        }

        public async Task<Result<List<BotSideUserWatchListProductsDto>>> GetUserProducts(long chatId)
        {
            var products = await _context.UserProductMapping
                      .AsNoTracking()
                      .Where(up => up.User.ChatId == chatId)
                      .Include(up => up.Product)
                      .Select(up => up.Product)
                      .Select(p => new BotSideUserWatchListProductsDto
                      {
                          Title = p.Title,
                          DigiKalaId = p.DigiKalaId
                      }).ToListAsync();
            if (!products.Any())
                return Result.Failure<List<BotSideUserWatchListProductsDto>>(BotMessages.ProductCheckListEmptyError);

            return products;
        }

        public async Task<Result<BotSideProductDto>> CreateProductForUser(BotSideCreateProductForUserDto dto)
        {
            if (await _context.Products.AnyAsync(p => p.DigiKalaId == dto.DigiKalaId))
                return Result.Failure<BotSideProductDto>(BotMessages.OperationFailedError);

            var digiKalaResult = await _digiKalaService.GetProductDetailsAsync(dto.DigiKalaId);
            if (digiKalaResult.IsFailure)
                return Result.Failure<BotSideProductDto>(digiKalaResult.Message!);

            var userIdResult = await _userService.GetUserIdByChatId(dto.ChatId);
            if (userIdResult.IsFailure)
                return Result.Failure<BotSideProductDto>(userIdResult.Message!);

            var product = new Product
            {
                DigiKalaId = dto.DigiKalaId,
                Title = digiKalaResult.Value.Title,
                OldPrice = 0,
                NewPrice = digiKalaResult.Value.Price,
                Url = $"https://www.digikala.com/product/dkp-{dto.DigiKalaId}/",
                LastUpdatedDate = DateTime.UtcNow,
                ImageUrl = digiKalaResult.Value.ImageUrl,
                ImageFileName = $"{dto.DigiKalaId}.jpg",
                PriceHistory = new List<ProductPriceHistory>
        {
            new() { Price = digiKalaResult.Value.Price }
        }
            };

            if (digiKalaResult.Value.ImageBytes is not null)
            {
                try
                {
                    await digiKalaResult.Value.ImageBytes.SaveImageFromByte(dto.DigiKalaId, AppInfo.ImageCacheFolder);
                }
                catch (Exception fileEx)
                {
                    _logger.LogWarning(fileEx, "Failed to write image to disk for new DKP: {DkpId}", dto.DigiKalaId);
                }
            }

            var userProduct = new UserProductMapping
            {
                UserId = userIdResult.Value,
                Product = product
            };
            _context.UserProductMapping.Add(userProduct);

            await _context.SaveChangesAsync();

            return new BotSideProductDto
            {
                Title = product.Title,
                DigiKalaId = product.DigiKalaId,
                ImageFileName = product.ImageFileName,
                NewPrice = product.NewPrice,
                OldPrice = product.OldPrice,
                LastUpdatedDate = product.LastUpdatedDate,
                Url = product.Url
            };
        }

        public async Task<Result<BotSideProductDto>> GetProductForUserByDigiKalaId(BotSideGetProductForUserDto dto)
        {
            var userIdResult = await _userService.GetUserIdByChatId(dto.ChatId);
            if (userIdResult.IsFailure)
                return Result.Failure<BotSideProductDto>(userIdResult.Message!);

            var product = await _context.UserProductMapping
                .AsNoTracking()
                .Where(upm => upm.UserId == userIdResult.Value && upm.Product.DigiKalaId == dto.DigiKalaId)
                .Select(upm => upm.Product)
                .Select(p => new BotSideProductDto()
                {
                    Id = p.Id,
                    DigiKalaId = p.DigiKalaId,
                    ImageFileName = p.ImageFileName,
                    LastUpdatedDate = p.LastUpdatedDate,
                    NewPrice = p.NewPrice,
                    OldPrice = p.OldPrice,
                    Title = p.Title,
                    Url = p.Url
                })
                .FirstOrDefaultAsync();
            if (product is null)
                return Result.Failure<BotSideProductDto>(BotMessages.YouDontCheckThisProductError);

            return product;
        }

        public async Task<List<Product>> GetProductsBatchForMonitoringAsync(BotSideGetProductsBatchForMonitoringDto dto)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => dto.ProductIds.Contains(p.Id))
                .ToListAsync(dto.CancellationToken);
        }
    }
}
