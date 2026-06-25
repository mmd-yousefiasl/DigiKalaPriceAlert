using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Products;
using Domain.Models.Users;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementaions
{
    public class ProductService : IProductService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDigiKalaService _digiKalaService;
        private readonly IUserService _userService;
        private readonly ILogger<IProductService> _logger;
        #endregion

        #region Ctor
        public ProductService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IDigiKalaService digiKalaService,
            IUserService userService,
            ILogger<IProductService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _digiKalaService = digiKalaService;
            _userService = userService;
            _logger = logger;
        }
        #endregion

        public async Task<Result<int>> ClientSideCreateNewProduct(ClientSideCreateNewProductDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Url))
                return Result.Failure<int>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<int>(Messages.OperationFailedError);

            var userResult = await _userService.ClientSideCanUserAddProduct();
            if (userResult.IsFailure)
                return Result.Failure<int>(userResult.Message!);

            var productIdResult = _digiKalaService.ExtractProductIdFromUrl(dto.Url);
            if (productIdResult.IsFailure)
                return Result.Failure<int>(Messages.InvalidDigiKalaProductUrlError);

            var digiKalaId = productIdResult.Value;

            var existingProductId = await _context.Products
                .AsNoTracking()
                .Where(n => n.DigiKalaId == digiKalaId)
                .Select(n => n.Id)
                .FirstOrDefaultAsync();

            if (existingProductId > 0)
            {
                var isAlreadyMapped = await _context.UserProductMapping
                    .AnyAsync(m => m.UserId == userId && m.ProductId == existingProductId);

                if (isAlreadyMapped)
                    return existingProductId;

                var userProductMapping = new UserProductMapping
                {
                    UserId = userId,
                    ProductId = existingProductId
                };

                _context.UserProductMapping.Add(userProductMapping);
                await _context.SaveChangesAsync();

                return existingProductId;
            }
            else
            {
                var digiKalaResult = await _digiKalaService.GetProductDetailsWithoutImage(digiKalaId);
                if (digiKalaResult.IsFailure)
                    return Result.Failure<int>(Messages.InvalidDigiKalaProductUrlError);

                var digiKala = digiKalaResult.Value;

                var product = new Product
                {
                    Title = digiKala.Title,
                    DigiKalaId = digiKalaId,
                    ImageUrl = digiKala.ImageUrl,
                    OldPrice = 0,
                    NewPrice = digiKala.Price,
                    Url = digiKalaId.GetDigiKalaProductUrl(),
                    Users = new List<UserProductMapping>{
                        new UserProductMapping()
                        {
                            UserId = userId,
                        },
                    },
                    PriceHistory = new List<ProductPriceHistory>{
                        new ProductPriceHistory()
                        {
                            Price = digiKala.Price,
                        }}
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return product.Id;
            }
        }

        public async Task<Result<ClientSideProductDetailsDto>> ClientSideGetProductDetails(int id)
        {
            if (id < 1)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            var todayUtc = DateTime.UtcNow.Date;

            var product = await _context.Products
                .AsNoTracking()
                .Where(n => n.Id == id)
                .Select(n => new ClientSideProductDetailsDto()
                {
                    Id = n.Id,
                    Title = n.Title,
                    DigiKalaId = n.DigiKalaId,
                    ImageUrl = n.ImageUrl,
                    NewPrice = n.NewPrice,
                    OldPrice = n.OldPrice,
                    Url = n.Url,
                    CreatedDate = n.CreatedDate,
                    LastUpdatedDate = n.LastUpdatedDate,
                    ChangePercent = n.ChangePercent,

                    AllTimeMaxPrice = n.PriceHistory.Max(ph => (decimal?)ph.Price) ?? 0,
                    AllTimeMinPrice = n.PriceHistory.Min(ph => (decimal?)ph.Price) ?? 0,

                    TodayPriceChangesCount = n.PriceHistory.Count(ph => ph.CreatedDate >= todayUtc),

                    History = n.PriceHistory
                        .OrderByDescending(ph => ph.CreatedDate)
                        .Select(ph => new ClientSideProductPriceHistoryDto()
                        {
                            Id = ph.Id,
                            Price = ph.Price,
                            CreatedDate = ph.CreatedDate
                        }).ToList()
                }).FirstOrDefaultAsync();

            if (product is null)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            return product;
        }

        public async Task<Result<ClientSideFilterProductsDto>> ClientSideFilterProducts(ClientSideFilterProductsDto filter)
        {
            if (filter is null)
                return Result.Failure<ClientSideFilterProductsDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideFilterProductsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Product>();

            conditions.Add(n => n.Users.Any(u => u.UserId == userId));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Title.Contains(filter.Search));
            }

            IQueryable<Product> query = _context.Products;

            foreach (var item in conditions)
            {
                query = query.Where(item);
            }

            var projectedQuery = query.Select(n => new ClientSideProductDto()
            {
                Id = n.Id,
                ImageUrl = n.ImageUrl,
                Title = n.Title,
                LastUpdatedDate = n.LastUpdatedDate,
                NewPrice = n.NewPrice,
                OldPrice = n.OldPrice,
                Url = n.Url,
                ChangePercent = n.ChangePercent,
                IsRead = n.Users.Where(u => u.UserId == userId).Select(u => u.IsRead).FirstOrDefault()
            });

            var sortedQuery = projectedQuery
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.LastUpdatedDate);

            await filter.Paging(sortedQuery);

            return filter;
        }

        public async Task<Result> ClientSideDeleteUserProduct(int id)
        {
            if (id < 1)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var userProduct = await _context.UserProductMapping
                .FirstOrDefaultAsync(n => n.UserId == userId && n.ProductId == id);
            if (userProduct is null)
                return Result.Failure(Messages.OperationFailedError);

            _context.UserProductMapping.Remove(userProduct);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<List<Product>> GetProductsBatchForMonitoringAsync(ClientSideGetProductsBatchForMonitoringDto dto)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => dto.ProductIds.Contains(p.Id))
                .ToListAsync(dto.CancellationToken);
        }

        public async Task<Result> ClientSideUpdateProductPrices(ClientSideUpdateProductPricesDto dto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(dto.CancellationToken);
                try
                {
                    decimal? calculatedPercent = dto.OldPrice.CalculateChangePercent(dto.NewPrice);

                    var rowsAffected = await _context.Products
                        .Where(p => p.Id == dto.ProductId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(p => p.OldPrice, p => p.NewPrice)
                            .SetProperty(p => p.NewPrice, dto.NewPrice)
                            .SetProperty(p => p.LastUpdatedDate, DateTime.UtcNow)
                            .SetProperty(p => p.ChangePercent, calculatedPercent),
                        dto.CancellationToken);

                    if (rowsAffected == 0)
                        return Result.Failure(Messages.OperationFailedError);

                    await _context.UserProductMapping
                        .Where(upm => upm.ProductId == dto.ProductId)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(upm => upm.IsRead, false),
                        dto.CancellationToken);

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
                    _logger.LogError(ex, "Error updating prices and notification flags for product {ProductId}", dto.ProductId);
                    throw;
                }
            });
        }

        public async Task<Result> ClientSideSetProductAsReadForUser(int id)
        {
            if (id < 1)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var rowsAffected = await _context.UserProductMapping
                .Where(n => n.UserId == userId && n.ProductId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(upm => upm.IsRead, true));

            if (rowsAffected == 0)
                return Result.Failure(Messages.OperationFailedError);

            return Result.Success();
        }
    }
}