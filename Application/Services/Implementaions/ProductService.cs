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

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id &&
                            !p.IsDeleted &&
                            p.Users.Any(u => u.UserId == userId && !u.IsDeleted))
                .Select(p => new ProductDetailsHeaderRow(
                    p.Id,
                    p.Title,
                    p.DigiKalaId,
                    p.Url,
                    p.ImageUrl,
                    p.CreatedDate,
                    p.LastUpdatedDate
                ))
                .FirstOrDefaultAsync();

            if (product is null)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            // ── Query 2: Active sellers (flat) ───────────────────────────────
            var sellers = await _context.ProductVariants
                .AsNoTracking()
                .Where(v => v.ProductId == id && v.IsActive && !v.IsDeleted)
                .SelectMany(v => v.Sellers
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .Select(s => new ProductSellerRow(
                        s.Id,
                        s.SellerName,
                        s.DigiKalaSellerId,
                        s.CurrentPrice,
                        s.PreviousPrice,
                        s.IsBuyBoxWinner,
                        s.SellerRating,
                        s.IsActive,
                        v.ColorName,
                        v.ColorHex,
                        s.LastUpdatedDate
                    )))
                .ToListAsync();

            if (sellers.Count == 0)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            // ── Query 3: Recent meaningful history only ──────────────────────
            var sellerIds = sellers.Select(s => s.SellerId).ToList();

            var historyRows = await _context.ProductPriceHistory
                .AsNoTracking()
                .Where(h =>
                    !h.IsDeleted &&
                    sellerIds.Contains(h.ProductVariantSellerId) &&
                    h.CreatedDate >= thirtyDaysAgo &&
                    (h.OldPrice != h.NewPrice || h.BuyBoxChanged))
                .OrderByDescending(h => h.CreatedDate)
                .Select(h => new SellerHistoryRow(
                    h.ProductVariantSellerId,
                    h.OldPrice,
                    h.NewPrice,
                    h.ChangePercent,
                    h.BuyBoxChanged,
                    h.IsBuyBoxWinner,
                    h.CreatedDate
                ))
                .ToListAsync();

            var historyBySeller = historyRows
                .GroupBy(h => h.ProductVariantSellerId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ── Build grouped color sections in memory ───────────────────────
            var colorGroups = BuildColorGroups(sellers, historyBySeller);

            // ── Final DTO ────────────────────────────────────────────────────
            var dto = new ClientSideProductDetailsDto
            {
                Id = product.Id,
                Title = product.Title,
                DigiKalaId = product.DigiKalaId,
                Url = product.Url,
                ImageUrl = product.ImageUrl,
                CreatedDate = product.CreatedDate,
                LastUpdatedDate = product.LastUpdatedDate,

                TotalVariantsCount = colorGroups.Count,
                TotalSellersCount = sellers.Count,

                LowestCurrentPrice = sellers
                    .Where(s => s.CurrentPrice > 0)
                    .Min(s => (decimal?)s.CurrentPrice) ?? 0,

                HighestCurrentPrice = sellers
                    .Where(s => s.CurrentPrice > 0)
                    .Max(s => (decimal?)s.CurrentPrice) ?? 0,

                ColorGroups = colorGroups
            };

            return Result.Success(dto);
        }

        #region Product Details Private Methods

        private List<ClientSideColorGroupDto> BuildColorGroups(
            List<ProductSellerRow> sellers,
            Dictionary<int, List<SellerHistoryRow>> historyBySeller)
        {
            return sellers
                .GroupBy(s => new { s.ColorName, s.ColorHex })
                .Select(group =>
                {
                    var sellerRows = group
                        .OrderByDescending(s => s.IsBuyBoxWinner)
                        .ThenBy(s => s.CurrentPrice)
                        .ToList();

                    var winner = sellerRows.FirstOrDefault(s => s.IsBuyBoxWinner)
                                 ?? sellerRows.FirstOrDefault();

                    var otherSellers = sellerRows
                        .Where(s => winner == null || s.SellerId != winner.SellerId)
                        .OrderBy(s => s.CurrentPrice)
                        .ToList();

                    var recentHistory = sellerRows
                        .SelectMany(s =>
                        {
                            historyBySeller.TryGetValue(s.SellerId, out var sellerHistory);

                            return (sellerHistory ?? new List<SellerHistoryRow>())
                                .Select(h => new ClientSideSellerPriceHistoryDto
                                {
                                    SellerName = s.SellerName,
                                    OldPrice = h.OldPrice,
                                    NewPrice = h.NewPrice,
                                    ChangePercent = h.ChangePercent,
                                    BuyBoxChanged = h.BuyBoxChanged,
                                    IsBuyBoxWinner = h.IsBuyBoxWinner,
                                    CreatedDate = h.CreatedDate
                                });
                        })
                        .OrderByDescending(h => h.CreatedDate)
                        .ToList();

                    return new ClientSideColorGroupDto
                    {
                        ColorName = group.Key.ColorName,
                        ColorHex = group.Key.ColorHex,
                        SellersCount = sellerRows.Count,
                        LastUpdatedDate = sellerRows.Max(s => s.LastUpdatedDate),

                        Winner = winner is null
                            ? null
                            : MapSeller(winner, historyBySeller),

                        OtherSellers = otherSellers
                            .Select(s => MapSeller(s, historyBySeller))
                            .ToList(),

                        RecentHistory = recentHistory
                    };
                })
                .OrderBy(g => g.ColorName == "بدون رنگ" ? 0 : 1)
                .ThenBy(g => g.Winner?.CurrentPrice ?? decimal.MaxValue)
                .ToList();
        }

        private static ClientSideVariantSellerDto MapSeller(
            ProductSellerRow seller,
            Dictionary<int, List<SellerHistoryRow>> historyBySeller)
        {
            historyBySeller.TryGetValue(seller.SellerId, out var sellerHistory);

            return new ClientSideVariantSellerDto
            {
                Id = seller.SellerId,
                SellerName = seller.SellerName,
                DigiKalaSellerId = seller.DigiKalaSellerId,
                CurrentPrice = seller.CurrentPrice,
                PreviousPrice = seller.PreviousPrice,
                IsBuyBoxWinner = seller.IsBuyBoxWinner,
                SellerRating = seller.SellerRating,
                IsActive = seller.IsActive,

                RecentHistory = (sellerHistory ?? new List<SellerHistoryRow>())
                    .Select(h => new ClientSideSellerPriceHistoryDto
                    {
                        SellerName = seller.SellerName,
                        OldPrice = h.OldPrice,
                        NewPrice = h.NewPrice,
                        ChangePercent = h.ChangePercent,
                        BuyBoxChanged = h.BuyBoxChanged,
                        IsBuyBoxWinner = h.IsBuyBoxWinner,
                        CreatedDate = h.CreatedDate
                    })
                    .ToList()
            };
        }

        private sealed record ProductDetailsHeaderRow(
            int Id,
            string Title,
            string DigiKalaId,
            string Url,
            string? ImageUrl,
            DateTime CreatedDate,
            DateTime LastUpdatedDate
        );

        private sealed record ProductSellerRow(
            int SellerId,
            string SellerName,
            string DigiKalaSellerId,
            decimal CurrentPrice,
            decimal? PreviousPrice,
            bool IsBuyBoxWinner,
            decimal? SellerRating,
            bool IsActive,
            string ColorName,
            string? ColorHex,
            DateTime LastUpdatedDate
        );

        private sealed record SellerHistoryRow(
            int ProductVariantSellerId,
            decimal OldPrice,
            decimal NewPrice,
            decimal ChangePercent,
            bool BuyBoxChanged,
            bool IsBuyBoxWinner,
            DateTime CreatedDate
        );

        #endregion

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
                            FirstName = upm.User.FirstName
                        })
                        .ToList()
                })
                .ToListAsync(ct);
        }

        public async Task<Result<List<ProductMonitoringTargetDto>>> GetProductBatchForMonitoringAsync(int page, int batchSize, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var batch = await _context.Products
                .AsNoTracking()
                .AsSplitQuery()
                .Where(p => !p.IsDeleted &&
                            p.Users.Any(upm =>
                                !upm.IsDeleted &&
                                !upm.User.IsUserBanned &&
                                upm.User.SubscriptionEndDate > now))
                .OrderBy(p => p.Id)
                .Skip(page * batchSize)
                .Take(batchSize)
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
                            FirstName = upm.User.FirstName
                        })
                        .ToList()
                }).ToListAsync(ct);

            if (!batch.Any())
                return Result.Failure<List<ProductMonitoringTargetDto>>(Messages.OperationFailedError);

            return Result.Success(batch);
        }

        public async Task<Result<List<ClientSideUserChangeNotification>>> SyncProductAndDetectChangesAsync(
            ProductMonitoringTargetDto target,
            ClientSideScrapedProductDto scraped,
            CancellationToken ct)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);

                try
                {
                    var now = DateTime.UtcNow;
                    var affectedUsers = new Dictionary<int, ClientSideUserChangeNotification>();
                    var anyChangeDetected = false;

                    // ── Load current DB state ────────────────────────────────────
                    var dbProduct = await _context.Products
                        .AsSplitQuery()
                        .Include(p => p.Variants.Where(v => !v.IsDeleted))
                            .ThenInclude(v => v.Sellers.Where(s => !s.IsDeleted))
                        .FirstOrDefaultAsync(p => p.Id == target.ProductId && !p.IsDeleted, ct);

                    if (dbProduct is null)
                        return Result.Failure<List<ClientSideUserChangeNotification>>(Messages.OperationFailedError);

                    // ── Build lookup maps ────────────────────────────────────────
                    var dbVariantMap = dbProduct.Variants
                        .ToDictionary(v => v.DigiKalaVariantId);

                    var dbSellerMap = dbProduct.Variants
                        .SelectMany(v => v.Sellers.Select(s => new
                        {
                            VariantDigiKalaId = v.DigiKalaVariantId,
                            Seller = s
                        }))
                        .ToDictionary(
                            x => (x.VariantDigiKalaId, x.Seller.DigiKalaSellerId),
                            x => x.Seller);

                    var scrapedSellerKeys = new HashSet<(long, string)>();

                    // ── Process each scraped variant ─────────────────────────────
                    foreach (var scrapedVariant in scraped.Variants)
                    {
                        var scrapedSellerId = scrapedVariant.SellerId.ToString();
                        scrapedSellerKeys.Add((scrapedVariant.DigiKalaVariantId, scrapedSellerId));

                        ProductVariant dbVariant;

                        if (dbVariantMap.TryGetValue(scrapedVariant.DigiKalaVariantId, out var existingVariant))
                        {
                            dbVariant = existingVariant;

                            if (dbVariant.IsActive != scrapedVariant.IsMarketable)
                            {
                                dbVariant.IsActive = scrapedVariant.IsMarketable;
                                dbVariant.LastUpdatedDate = now;
                            }

                            dbVariant.ColorName = scrapedVariant.ColorName;
                            dbVariant.ColorHex = scrapedVariant.ColorHex;
                        }
                        else
                        {
                            // ── New variant ──────────────────────────────────────
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

                        if (dbSellerMap.TryGetValue((scrapedVariant.DigiKalaVariantId, scrapedSellerId), out var dbSeller))
                        {
                            var oldPrice = dbSeller.CurrentPrice;
                            var oldBuyBox = dbSeller.IsBuyBoxWinner;

                            var priceChanged = oldPrice != scrapedVariant.Price;
                            var buyBoxChanged = oldBuyBox != scrapedVariant.IsBuyBoxWinner;

                            // Always keep metadata fresh
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

                                changePercent = Math.Max(-99999, Math.Min(99999, changePercent));

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

                                // ── Notify all watchers — no filtering ───────────
                                foreach (var watcher in target.Watchers)
                                {
                                    if (!affectedUsers.ContainsKey(watcher.UserId))
                                    {
                                        affectedUsers[watcher.UserId] = new ClientSideUserChangeNotification
                                        {
                                            UserId = watcher.UserId
                                        };
                                    }

                                    if (priceChanged)
                                        affectedUsers[watcher.UserId].HasPriceChange = true;

                                    if (buyBoxChanged)
                                        affectedUsers[watcher.UserId].HasBuyBoxChange = true;
                                }
                            }
                        }
                        else
                        {
                            // ── New seller on existing variant ───────────────────
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

                    // ── Mark disappeared sellers as inactive ─────────────────────
                    foreach (var dbVariant in dbProduct.Variants)
                    {
                        foreach (var dbSeller in dbVariant.Sellers.Where(s => s.IsActive))
                        {
                            if (!scrapedSellerKeys.Contains((dbVariant.DigiKalaVariantId, dbSeller.DigiKalaSellerId)))
                            {
                                dbSeller.IsActive = false;
                                dbSeller.LastUpdatedDate = now;
                                anyChangeDetected = true;
                            }
                        }
                    }

                    // ── Update product timestamp + reset IsRead ──────────────────
                    if (anyChangeDetected)
                    {
                        dbProduct.LastUpdatedDate = now;

                        await _context.UserProductMappings
                            .Where(m => m.ProductId == target.ProductId && !m.IsDeleted)
                            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, false), ct);
                    }

                    await _context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);

                    return affectedUsers.Values.ToList();
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
        }
    }
}