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

        public async Task<Result<List<Product>>> GetUserProductsAsync(long chatId)
        {
            var products = await _context.UserProductMapping
                .Where(up => up.UserId == chatId)
                .Include(up => up.Product)
                .Select(up => up.Product)
                .ToListAsync();

            return products;
        }

        public async Task<Result> RemoveProductFromUserAsync(BotSideProductUserInfoDto dto)
        {
            var userProduct = await _context.UserProductMapping
                .FirstOrDefaultAsync(up => up.User.ChatId == dto.ChatId && up.Product.DigiKalaId == dto.DigiKalaId);

            if (userProduct is null)
                return Result.Failure(AppStatics.ProductNotFoundInServerError);

            _context.UserProductMapping.Remove(userProduct);
            await _context.SaveChangesAsync();

            return Result.Success(AppStatics.ProductDeletedSuccessfullyMessage);
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
                return Result.Failure<BotSideProductDto>(AppStatics.YouDontCheckThisProductError);

            return product;
        }

        public async Task<List<Product>> GetProductsBatchForMonitoringAsync(BotSideGetProductsBatchForMonitoringDto dto)
        {
            return await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Skip(dto.Page * dto.BarchSize)
                .Take(dto.BarchSize)
                .ToListAsync(dto.CancellationToken);
        }

        public async Task UpdateProductPrices(BotSideUpdateProductPricesDto dto)
        {
            await _context.Products
                .Where(p => p.Id == dto.ProductId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.OldPrice, p => p.NewPrice)
                    .SetProperty(p => p.NewPrice, dto.NewPrice)
                    .SetProperty(p => p.LastUpdatedDate, DateTime.UtcNow), dto.CancellationToken);
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
                return Result.Failure<List<BotSideUserWatchListProductsDto>>(AppStatics.ProductCheckListEmptyError);

            return products;
        }

        public async Task<Result<BotSideProductDto>> CreateProductForUser(BotSideCreateProductForUserDto dto)
        {
            if (await _context.Products.AnyAsync(p => p.DigiKalaId == dto.DigiKalaId))
                return Result.Failure<BotSideProductDto>(AppStatics.OperationFailedError);

            var digiKalaResult = await _digiKalaService.GetProductDetailsAsync(dto.DigiKalaId);
            if (digiKalaResult.IsFailure)
                return Result.Failure<BotSideProductDto>(digiKalaResult.Message!);

            var product = new Product
            {
                DigiKalaId = dto.DigiKalaId,
                Title = digiKalaResult.Value.Title,
                OldPrice = null,
                NewPrice = digiKalaResult.Value.Price,
                Url = $"https://www.digikala.com/product/dkp-{dto.DigiKalaId}/",
                LastUpdatedDate = DateTime.UtcNow,
                ImageFileName = $"{dto.DigiKalaId}.jpg"
            };

            _context.Products.Add(product);

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

            var userIdResult = await _userService.GetUserIdByChatId(dto.ChatId);
            if (userIdResult.IsFailure)
                return Result.Failure<BotSideProductDto>(userIdResult.Message!);

            var userProduct = new UserProductMapping
            {
                UserId = userIdResult.Value,
                Product = product
            };
            _context.UserProductMapping.Add(userProduct);

            await _context.SaveChangesAsync();

            return new BotSideProductDto()
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
                return Result.Failure<BotSideProductDto>(AppStatics.YouDontCheckThisProductError);

            return product;
        }
    }
}
