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
    .Where(h => h.OldPrice != h.NewPrice || h.BuyBoxChanged) // ← only real changes
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
    }).ToList()
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

        public async Task<List<ProductMonitoringTargetDto>> GetAllProductsForMonitoringAsync(CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            return await _context.Products
                .AsNoTracking()
                .AsSplitQuery()
                .Where(p => !p.IsDeleted &&
                            p.Users.Any(upm =>
                                !upm.IsDeleted &&
                                !upm.User.IsUserBanned &&
                                upm.User.SubscriptionEndDate > now))
                .Select(p => new ProductMonitoringTargetDto
                {
                    ProductId = p.Id,
                    DigiKalaId = p.DigiKalaId,
                    Watchers = p.Users
                        .Where(upm =>
                            !upm.IsDeleted &&
                            !upm.User.IsUserBanned &&
                            upm.User.SubscriptionEndDate > now)
                        .Select(upm => new ProductWatcherDto
                        {
                            UserId = upm.UserId,
                            Mobile = upm.User.Mobile,
                            FirstName = upm.User.FirstName,
                            WatchMode = upm.WatchMode,
                            NotifyOnPriceChange = upm.NotifyOnPriceChange,
                            NotifyOnBuyBoxChange = upm.NotifyOnBuyBoxChange,
                            MinPriceChangeThreshold = upm.MinPriceChangeThreshold,
                            WatchedVariantIds = upm.VariantWatches
                                .Select(vw => vw.ProductVariantId)
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync(ct);
        }

        public async Task<List<int>> SyncProductAndDetectChangesAsync(ProductMonitoringTargetDto target, ClientSideScrapedProductDto scraped, CancellationToken ct)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);

                try
                {
                    var now = DateTime.UtcNow;
                    var affectedUserIds = new HashSet<int>();
                    var anyChangeDetected = false;

                    // ── Load current DB state for this product ───────────────────
                    var dbProduct = await _context.Products
                    .AsSplitQuery()
                        .Include(p => p.Variants.Where(v => !v.IsDeleted))
                            .ThenInclude(v => v.Sellers.Where(s => !s.IsDeleted))
                        .FirstOrDefaultAsync(p => p.Id == target.ProductId && !p.IsDeleted, ct);

                    if (dbProduct is null)
                        return new List<int>();

                    // ── Build lookup maps ───────────────────────────────────────
                    var dbVariantMap = dbProduct.Variants
                        .ToDictionary(v => v.DigiKalaVariantId);

                    var dbSellerMap = dbProduct.Variants
                        .SelectMany(v => v.Sellers.Select(s => new
                        {
                            VariantId = v.Id,
                            VariantDigiKalaId = v.DigiKalaVariantId,
                            Seller = s
                        }))
                        .ToDictionary(
                            x => (x.VariantDigiKalaId, x.Seller.DigiKalaSellerId),
                            x => x.Seller);

                    // Track existing seller keys from scrape
                    var scrapedSellerKeys = new HashSet<(long VariantDigiKalaId, string SellerId)>();

                    // ── Process scraped variants ────────────────────────────────
                    foreach (var scrapedVariant in scraped.Variants)
                    {
                        var scrapedSellerId = scrapedVariant.SellerId.ToString();
                        scrapedSellerKeys.Add((scrapedVariant.DigiKalaVariantId, scrapedSellerId));

                        // ── Find or create variant ──────────────────────────────
                        ProductVariant dbVariant;

                        if (dbVariantMap.TryGetValue(scrapedVariant.DigiKalaVariantId, out var existingVariant))
                        {
                            dbVariant = existingVariant;

                            if (dbVariant.IsActive != scrapedVariant.IsMarketable)
                            {
                                dbVariant.IsActive = scrapedVariant.IsMarketable;
                                dbVariant.LastUpdatedDate = now;
                            }

                            // keep metadata fresh
                            dbVariant.ColorName = scrapedVariant.ColorName;
                            dbVariant.ColorHex = scrapedVariant.ColorHex;
                        }
                        else
                        {
                            // New variant → create full graph including seller + initial history
                            dbVariant = new ProductVariant
                            {
                                ProductId = dbProduct.Id,
                                DigiKalaVariantId = scrapedVariant.DigiKalaVariantId,
                                ColorName = scrapedVariant.ColorName,
                                ColorHex = scrapedVariant.ColorHex,
                                IsActive = scrapedVariant.IsMarketable,
                                CreatedDate = now,
                                LastUpdatedDate = now,
                                Sellers = new List<ProductVariantSeller>
                        {
                            new ProductVariantSeller
                            {
                                DigiKalaSellerId = scrapedSellerId,
                                SellerName = scrapedVariant.SellerName,
                                CurrentPrice = scrapedVariant.Price,
                                PreviousPrice = null,
                                IsBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                                WasBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                                SellerRating = scrapedVariant.SellerRating,
                                IsActive = true,
                                CreatedDate = now,
                                LastUpdatedDate = now,
                                PriceHistory = new List<ProductPriceHistory>
                                {
                                    new ProductPriceHistory
                                    {
                                        OldPrice = scrapedVariant.Price,
                                        NewPrice = scrapedVariant.Price,
                                        ChangePercent = 0,
                                        BuyBoxChanged = false,
                                        IsBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                                        CreatedDate = now,
                                        LastUpdatedDate = now
                                    }
                                }
                            }
                        }
                            };

                            dbProduct.Variants.Add(dbVariant);
                            anyChangeDetected = true;

                            continue;
                        }

                        // ── Find or create seller on existing variant ──────────
                        if (dbSellerMap.TryGetValue((scrapedVariant.DigiKalaVariantId, scrapedSellerId), out var dbSeller))
                        {
                            var oldPrice = dbSeller.CurrentPrice;
                            var oldBuyBox = dbSeller.IsBuyBoxWinner;

                            var priceChanged = oldPrice != scrapedVariant.Price;
                            var buyBoxChanged = oldBuyBox != scrapedVariant.IsBuyBoxWinner;

                            // keep metadata fresh even if no price change
                            dbSeller.SellerName = scrapedVariant.SellerName;
                            dbSeller.SellerRating = scrapedVariant.SellerRating;
                            dbSeller.IsActive = true;

                            if (priceChanged || buyBoxChanged)
                            {
                                dbSeller.PreviousPrice = oldPrice;
                                dbSeller.CurrentPrice = scrapedVariant.Price;
                                dbSeller.WasBuyBoxWinner = oldBuyBox;
                                dbSeller.IsBuyBoxWinner = scrapedVariant.IsBuyBoxWinner;
                                dbSeller.LastUpdatedDate = now;

                                var changePercent = oldPrice > 0
                                    ? Math.Round((scrapedVariant.Price - oldPrice) / oldPrice * 100, 4)
                                    : 0;

                                dbSeller.PriceHistory.Add(new ProductPriceHistory
                                {
                                    OldPrice = oldPrice,
                                    NewPrice = scrapedVariant.Price,
                                    ChangePercent = changePercent,
                                    BuyBoxChanged = buyBoxChanged,
                                    IsBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                                    CreatedDate = now,
                                    LastUpdatedDate = now
                                });

                                anyChangeDetected = true;

                                foreach (var watcher in target.Watchers)
                                {
                                    if (ShouldNotifyUser(
                                        watcher,
                                        dbVariant.Id,
                                        priceChanged,
                                        buyBoxChanged,
                                        scrapedVariant.Price,
                                        oldPrice))
                                    {
                                        affectedUserIds.Add(watcher.UserId);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // New seller on existing variant
                            dbVariant.Sellers.Add(new ProductVariantSeller
                            {
                                DigiKalaSellerId = scrapedSellerId,
                                SellerName = scrapedVariant.SellerName,
                                CurrentPrice = scrapedVariant.Price,
                                PreviousPrice = null,
                                IsBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                                WasBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                                SellerRating = scrapedVariant.SellerRating,
                                IsActive = true,
                                CreatedDate = now,
                                LastUpdatedDate = now,
                                PriceHistory = new List<ProductPriceHistory>
                        {
                            new ProductPriceHistory
                            {
                                OldPrice = scrapedVariant.Price,
                                NewPrice = scrapedVariant.Price,
                                ChangePercent = 0,
                                BuyBoxChanged = false,
                                IsBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                                CreatedDate = now,
                                LastUpdatedDate = now
                            }
                        }
                            });

                            anyChangeDetected = true;
                        }
                    }

                    // ── Mark disappeared sellers as inactive ────────────────────
                    foreach (var dbVariant in dbProduct.Variants)
                    {
                        foreach (var dbSeller in dbVariant.Sellers.Where(s => s.IsActive))
                        {
                            var sellerKey = (dbVariant.DigiKalaVariantId, dbSeller.DigiKalaSellerId);

                            if (!scrapedSellerKeys.Contains(sellerKey))
                            {
                                dbSeller.IsActive = false;
                                dbSeller.LastUpdatedDate = now;
                                anyChangeDetected = true;
                            }
                        }
                    }

                    // ── Update product + IsRead flags if anything changed ───────
                    if (anyChangeDetected)
                    {
                        dbProduct.LastUpdatedDate = now;

                        await _context.UserProductMappings
                            .Where(m => m.ProductId == target.ProductId && !m.IsDeleted)
                            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, false), ct);
                    }

                    await _context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    return affectedUserIds.ToList();
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
        }

        private static bool ShouldNotifyUser(ProductWatcherDto watcher, int variantId, bool priceChanged, bool buyBoxChanged, decimal newPrice, decimal oldPrice)
        {
            var variantIsWatched = watcher.WatchMode switch
            {
                WatchMode.AllVariants => true,
                WatchMode.SpecificVariants => watcher.WatchedVariantIds.Contains(variantId),
                WatchMode.MyShopVariantsOnly => watcher.WatchedVariantIds.Contains(variantId),
                _ => false
            };

            if (!variantIsWatched)
                return false;

            if (priceChanged && watcher.NotifyOnPriceChange)
            {
                if (watcher.MinPriceChangeThreshold.HasValue)
                {
                    var diff = Math.Abs(newPrice - oldPrice);
                    if (diff >= watcher.MinPriceChangeThreshold.Value)
                        return true;
                }
                else
                {
                    return true;
                }
            }

            if (buyBoxChanged && watcher.NotifyOnBuyBoxChange)
                return true;

            return false;
        }
    }
}