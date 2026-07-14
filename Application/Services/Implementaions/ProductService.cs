using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.Products;
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

        /*        public async Task<Result<int>> ClientSideCreateNewProduct(ClientSideCreateNewProductDto dto)
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
                        var digiKalaResult = await _digiKalaService.GetProductDetails(digiKalaId);
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
                        .Where(n => n.Id == id && n.Users.Any(u => u.UserId == userId))
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
                                }).Take(50)
                                .ToList()
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

                    if (filter.SortType == ProductSortTypeEnum.UnreadOnly)
                    {
                        conditions.Add(n => n.Users.Any(u => u.UserId == userId && !u.IsRead));
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

                    IOrderedQueryable<ClientSideProductDto> sortedQuery;

                    switch (filter.SortType)
                    {
                        case ProductSortTypeEnum.OldestUpdate:
                            sortedQuery = projectedQuery.OrderBy(n => n.LastUpdatedDate);
                            break;

                        case ProductSortTypeEnum.HighestPrice:
                            sortedQuery = projectedQuery.OrderByDescending(n => n.NewPrice);
                            break;

                        case ProductSortTypeEnum.LowestPrice:
                            sortedQuery = projectedQuery.OrderBy(n => n.NewPrice);
                            break;

                        case ProductSortTypeEnum.HighestPriceDrop:
                            sortedQuery = projectedQuery.OrderBy(n => n.ChangePercent);
                            break;

                        case ProductSortTypeEnum.UnreadOnly:
                            sortedQuery = projectedQuery.OrderByDescending(n => n.LastUpdatedDate);
                            break;

                        case ProductSortTypeEnum.NewestUpdate:
                        default:
                            sortedQuery = projectedQuery.OrderBy(n => n.IsRead).ThenByDescending(n => n.LastUpdatedDate);
                            break;
                    }

                    await filter.Paging(sortedQuery);

                    return filter;
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

             */

        public async Task<Result<int>> ClientSideCreateNewProduct(ClientSideCreateNewProductDto dto)
        {
            if (dto is null)
                return Result.Failure<int>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<int>(Messages.OperationFailedError);

            var userResult = await _userService.ClientSideCanUserAddProduct();
            if (userResult.IsFailure)
                return Result.Failure<int>(userResult.Message!);

            var extractResult = _digiKalaService.ExtractProductIdFromUrl(dto.Url);
            if (extractResult.IsFailure)
                return Result.Failure<int>(Messages.InvalidDigiKalaProductUrlError);

            var digiKalaId = extractResult.Value;

            var existingProduct = await _context.Products
                .AsNoTracking()
                .Where(p => p.DigiKalaId == digiKalaId)
                .Select(p => new { p.Id })
                .FirstOrDefaultAsync();

            if (existingProduct is not null)
                return await MapUserToExistingProductAsync(userId, existingProduct.Id);

            return await CreateProductFromScrapeAsync(userId, digiKalaId);
        }

        #region Create New Product Private Methods
        private async Task<Result<int>> MapUserToExistingProductAsync(int userId, int productId)
        {
            var alreadyMapped = await _context.UserProductMappings
                .AnyAsync(m => m.UserId == userId && m.ProductId == productId);

            if (alreadyMapped)
                return productId;

            var mapping = new UserProductMapping
            {
                UserId = userId,
                ProductId = productId,
                WatchMode = WatchMode.AllVariants,
                NotifyOnPriceChange = true,
                NotifyOnBuyBoxChange = true,
                NotifyOnNewSeller = false,
                NotifyOnSellerLeft = false
            };

            _context.UserProductMappings.Add(mapping);
            await _context.SaveChangesAsync();

            return productId;
        }

        private async Task<Result<int>> CreateProductFromScrapeAsync(int userId, string digiKalaId)
        {
            var scrapeResult = await _digiKalaService.ScrapeFullProductAsync(digiKalaId);
            if (scrapeResult.IsFailure)
                return Result.Failure<int>(Messages.InvalidDigiKalaProductUrlError);

            var scraped = scrapeResult.Value;

            var product = new Product
            {
                DigiKalaId = digiKalaId,
                Title = scraped.Title,
                Url = scraped.Url,
                ImageUrl = scraped.ImageUrl
            };

            foreach (var scrapedVariant in scraped.Variants)
            {
                var variant = new ProductVariant
                {
                    DigiKalaVariantId = scrapedVariant.DigiKalaVariantId,
                    ColorName = scrapedVariant.ColorName,
                    ColorHex = scrapedVariant.ColorHex,
                    IsActive = scrapedVariant.IsMarketable
                };

                var seller = new ProductVariantSeller
                {
                    DigiKalaSellerId = scrapedVariant.SellerId.ToString(),
                    SellerName = scrapedVariant.SellerName,
                    CurrentPrice = scrapedVariant.Price,
                    PreviousPrice = null,
                    IsBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                    WasBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                    SellerRating = scrapedVariant.SellerRating,
                    IsActive = scrapedVariant.IsMarketable
                };

                var initialHistory = new ProductPriceHistory
                {
                    OldPrice = scrapedVariant.Price,
                    NewPrice = scrapedVariant.Price,
                    ChangePercent = 0,
                    BuyBoxChanged = false,
                    IsBuyBoxWinner = scrapedVariant.IsBuyBoxWinner
                };

                seller.PriceHistory = new List<ProductPriceHistory> { initialHistory };
                variant.Sellers = new List<ProductVariantSeller> { seller };
                product.Variants.Add(variant);
            }

            var userMapping = new UserProductMapping
            {
                UserId = userId,
                WatchMode = WatchMode.AllVariants,
                NotifyOnPriceChange = true,
                NotifyOnBuyBoxChange = true,
                NotifyOnNewSeller = false,
                NotifyOnSellerLeft = false
            };

            product.Users = new List<UserProductMapping> { userMapping };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product.Id;
        }
        #endregion

        public async Task<Result<ClientSideProductDetailsDto>> ClientSideGetProductDetails(int id)
        {
            if (id < 1)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            // ── Step 1: Fetch raw data from DB ───────────────────────────────
            var raw = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id && p.Users.Any(u => u.UserId == userId))
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.DigiKalaId,
                    p.Url,
                    p.ImageUrl,
                    p.CreatedDate,
                    p.LastUpdatedDate,

                    Variants = p.Variants
                        .Where(v => v.IsActive)
                        .Select(v => new
                        {
                            v.Id,
                            v.DigiKalaVariantId,
                            v.ColorName,
                            v.ColorHex,
                            v.IsActive,

                            Sellers = v.Sellers
                                .Where(s => s.IsActive)
                                .Select(s => new
                                {
                                    s.Id,
                                    s.SellerName,
                                    s.DigiKalaSellerId,
                                    s.CurrentPrice,
                                    s.PreviousPrice,
                                    s.IsBuyBoxWinner,
                                    s.SellerRating,
                                    s.IsActive,

                                    // ✅ Fetch flat list, sort/take in memory
                                    History = s.PriceHistory
                                        .Select(h => new
                                        {
                                            h.OldPrice,
                                            h.NewPrice,
                                            h.ChangePercent,
                                            h.BuyBoxChanged,
                                            h.IsBuyBoxWinner,
                                            h.CreatedDate
                                        })
                                        .ToList()
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (raw is null)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            // ── Step 2: Project in memory (safe to use OrderBy/Take here) ───
            var allSellers = raw.Variants.SelectMany(v => v.Sellers).ToList();

            var dto = new ClientSideProductDetailsDto
            {
                Id = raw.Id,
                Title = raw.Title,
                DigiKalaId = raw.DigiKalaId,
                Url = raw.Url,
                ImageUrl = raw.ImageUrl,
                CreatedDate = raw.CreatedDate,
                LastUpdatedDate = raw.LastUpdatedDate,

                TotalVariantsCount = raw.Variants.Count,
                TotalSellersCount = allSellers.Count,

                LowestCurrentPrice = allSellers
                    .Where(s => s.CurrentPrice > 0)
                    .Min(s => (decimal?)s.CurrentPrice) ?? 0,

                HighestCurrentPrice = allSellers
                    .Where(s => s.CurrentPrice > 0)
                    .Max(s => (decimal?)s.CurrentPrice) ?? 0,

                BuyBoxSellerName = allSellers
                    .FirstOrDefault(s => s.IsBuyBoxWinner)?.SellerName,

                BuyBoxPrice = allSellers
                    .FirstOrDefault(s => s.IsBuyBoxWinner)?.CurrentPrice,

                BuyBoxColorName = raw.Variants
                    .FirstOrDefault(v => v.Sellers.Any(s => s.IsBuyBoxWinner))?.ColorName,

                Variants = raw.Variants
                    .SelectMany(v => v.Sellers.Select(s => new { Variant = v, Seller = s }))
                    .OrderByDescending(x => x.Seller.IsBuyBoxWinner)
                    .ThenBy(x => x.Seller.CurrentPrice)
                    .Select(x => new ClientSideVariantDetailsDto
                    {
                        Id = x.Variant.Id,
                        DigiKalaVariantId = x.Variant.DigiKalaVariantId,
                        ColorName = x.Variant.ColorName,
                        ColorHex = x.Variant.ColorHex,
                        IsActive = x.Variant.IsActive,

                        Seller = new ClientSideVariantSellerDto
                        {
                            Id = x.Seller.Id,
                            SellerName = x.Seller.SellerName,
                            DigiKalaSellerId = x.Seller.DigiKalaSellerId,
                            CurrentPrice = x.Seller.CurrentPrice,
                            PreviousPrice = x.Seller.PreviousPrice,
                            IsBuyBoxWinner = x.Seller.IsBuyBoxWinner,
                            SellerRating = x.Seller.SellerRating,
                            IsActive = x.Seller.IsActive,

                            // ✅ OrderBy + Take now happen in memory
                            RecentHistory = x.Seller.History
                                .OrderByDescending(h => h.CreatedDate)
                                .Take(20)
                                .Select(h => new ClientSideSellerPriceHistoryDto
                                {
                                    OldPrice = h.OldPrice,
                                    NewPrice = h.NewPrice,
                                    ChangePercent = h.ChangePercent,
                                    BuyBoxChanged = h.BuyBoxChanged,
                                    IsBuyBoxWinner = h.IsBuyBoxWinner,
                                    CreatedDate = h.CreatedDate
                                })
                                .ToList()
                        }
                    })
                    .ToList()
            };

            return dto;
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
                conditions.Add(n => n.Title.Contains(filter.Search));

            if (filter.SortType == ProductSortTypeEnum.UnreadOnly)
                conditions.Add(n => n.Users.Any(u => u.UserId == userId && !u.IsRead));

            IQueryable<Product> query = _context.Products;

            foreach (var item in conditions)
                query = query.Where(item);

            var projectedQuery = query.Select(n => new ClientSideProductDto
            {
                Id = n.Id,
                Title = n.Title,
                ImageUrl = n.ImageUrl,
                Url = n.Url,
                LastUpdatedDate = n.LastUpdatedDate,

                IsRead = n.Users
                    .Where(u => u.UserId == userId)
                    .Select(u => u.IsRead)
                    .FirstOrDefault(),

                LowestPrice = n.Variants
                    .SelectMany(v => v.Sellers)
                    .Where(s => s.IsActive && s.CurrentPrice > 0)
                    .Min(s => (decimal?)s.CurrentPrice) ?? 0,

                HighestPrice = n.Variants
                    .SelectMany(v => v.Sellers)
                    .Where(s => s.IsActive && s.CurrentPrice > 0)
                    .Max(s => (decimal?)s.CurrentPrice) ?? 0,

                VariantsCount = n.Variants.Count(v => v.IsActive),

                SellersCount = n.Variants
                    .SelectMany(v => v.Sellers)
                    .Count(s => s.IsActive)
            });

            IOrderedQueryable<ClientSideProductDto> sortedQuery;

            switch (filter.SortType)
            {
                case ProductSortTypeEnum.OldestUpdate:
                    sortedQuery = projectedQuery.OrderBy(n => n.LastUpdatedDate);
                    break;

                case ProductSortTypeEnum.HighestPrice:
                    sortedQuery = projectedQuery.OrderByDescending(n => n.HighestPrice);
                    break;

                case ProductSortTypeEnum.LowestPrice:
                    sortedQuery = projectedQuery.OrderBy(n => n.LowestPrice);
                    break;

                case ProductSortTypeEnum.UnreadOnly:
                    sortedQuery = projectedQuery.OrderByDescending(n => n.LastUpdatedDate);
                    break;

                case ProductSortTypeEnum.NewestUpdate:
                default:
                    sortedQuery = projectedQuery
                        .OrderBy(n => n.IsRead)
                        .ThenByDescending(n => n.LastUpdatedDate);
                    break;
            }

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

            var userProduct = await _context.UserProductMappings
                .FirstOrDefaultAsync(n => n.UserId == userId && n.ProductId == id);
            if (userProduct is null)
                return Result.Failure(Messages.OperationFailedError);

            _context.UserProductMappings.Remove(userProduct);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public Task<List<Product>> GetProductsBatchForMonitoringAsync(ClientSideGetProductsBatchForMonitoringDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<Result> ClientSideUpdateProductPrices(ClientSideUpdateProductPricesDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> ClientSideSetProductAsReadForUser(int id)
        {
            if (id < 1)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var rowsAffected = await _context.UserProductMappings
                .Where(n => n.UserId == userId && n.ProductId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(upm => upm.IsRead, true));

            if (rowsAffected == 0)
                return Result.Failure(Messages.OperationFailedError);

            return Result.Success();
        }
    }
}