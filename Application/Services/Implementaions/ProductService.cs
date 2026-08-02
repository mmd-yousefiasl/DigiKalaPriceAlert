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
using Npgsql;

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

        public async Task<Result<ClientSideProductDetailsDto>> ClientSideGetProductDetails(int id)
        {
            if (id < 1)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var trackingContext = await _context.UserProductMappings
                .AsNoTracking()
                .Where(m => m.Id == id && m.UserId == userId && !m.IsDeleted)
                .Select(m => new
                {
                    ProductId = m.VariantSeller.ProductId,
                    Title = m.VariantSeller.Product.Title,
                    DigiKalaId = m.VariantSeller.Product.DigiKalaId,
                    ImageUrl = m.VariantSeller.Product.ImageUrl,
                    Url = m.VariantSeller.Product.Url,
                    LastUpdatedDate = m.VariantSeller.Product.LastUpdatedDate,

                    VariantColorName = m.VariantSeller.ColorName,
                    VariantColorHex = m.VariantSeller.ColorHex,
                    TrackedSellerId = m.VariantSellerId
                })
                .FirstOrDefaultAsync();

            if (trackingContext is null)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            var sellers = await _context.ProductVariantSellers
                .AsNoTracking()
                .Where(s => s.ProductId == trackingContext.ProductId
                         && s.ColorName == trackingContext.VariantColorName
                         && !s.IsDeleted)
                .Select(s => new ClientSideVariantSellerDto
                {
                    Id = s.Id,
                    SellerName = s.SellerName,
                    CurrentPrice = s.CurrentPrice,
                    PreviousPrice = s.PreviousPrice,
                    IsBuyBoxWinner = s.IsBuyBoxWinner,
                    IsUserTracked = (s.Id == trackingContext.TrackedSellerId),
                    IsActive = s.IsActive,
                    SellerRating = s.SellerRating
                })
                .ToListAsync();

            if (sellers.Count == 0)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            var winnerId = sellers.FirstOrDefault(s => s.IsBuyBoxWinner)?.Id ?? 0;

            var sellerIds = sellers.Select(s => s.Id).ToList();

            var history = await _context.ProductPriceHistory
                .AsNoTracking()
                .Where(h => !h.IsDeleted &&
                            sellerIds.Contains(h.ProductVariantSellerId) &&
                            h.CreatedDate >= thirtyDaysAgo &&
                            (h.OldPrice != h.NewPrice || h.BuyBoxChanged))
                .OrderByDescending(h => h.CreatedDate)
                .Select(h => new ClientSideSellerPriceHistoryDto
                {
                    SellerName = h.VariantSeller.SellerName,
                    OldPrice = h.OldPrice,
                    NewPrice = h.NewPrice,
                    ChangePercent = h.ChangePercent,
                    BuyBoxChanged = h.BuyBoxChanged,
                    IsBuyBoxWinner = h.IsBuyBoxWinner,
                    CreatedDate = h.CreatedDate,
                    ChangeDescription = h.ChangeDescription
                })
                .ToListAsync();

            var dto = new ClientSideProductDetailsDto
            {
                ProductId = trackingContext.ProductId,
                Title = trackingContext.Title,
                DigiKalaId = trackingContext.DigiKalaId,
                ImageUrl = trackingContext.ImageUrl,
                Url = trackingContext.Url,
                LastUpdatedDate = trackingContext.LastUpdatedDate,
                VariantColorName = trackingContext.VariantColorName,
                VariantColorHex = trackingContext.VariantColorHex,
                TotalActiveSellersInVariant = sellers.Count(s => s.IsActive),

                LowestCurrentPrice = sellers.Where(s => s.CurrentPrice > 0 && s.IsActive).Min(s => (decimal?)s.CurrentPrice) ?? 0,
                HighestCurrentPrice = sellers.Where(s => s.CurrentPrice > 0 && s.IsActive).Max(s => (decimal?)s.CurrentPrice) ?? 0,

                TrackedSeller = sellers.FirstOrDefault(s => s.Id == trackingContext.TrackedSellerId),
                WinnerSeller = sellers.FirstOrDefault(s => s.Id == winnerId),

                Opponents = sellers
                    .Where(s => s.Id != trackingContext.TrackedSellerId && s.Id != winnerId)
                    .OrderBy(s => s.CurrentPrice)
                    .ToList(),

                RecentHistory = history
            };

            return Result.Success(dto);
        }

        public async Task<Result<ClientSideFilterProductsDto>> ClientSideFilterProducts(ClientSideFilterProductsDto filter)
        {
            if (filter is null)
                return Result.Failure<ClientSideFilterProductsDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideFilterProductsDto>(Messages.OperationFailedError);

            IQueryable<UserProductMapping> query = _context.UserProductMappings
                .Where(m => m.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                // Updated: Direct path to Product
                query = query.Where(m => m.VariantSeller.Product.Title.Contains(filter.Search));
            }

            // Unread Filter
            if (filter.SortType == ProductSortTypeEnum.UnreadOnly)
            {
                query = query.Where(m => !m.IsRead);
            }

            var projectedQuery = query.Select(m => new ClientSideProductDto
            {
                Id = m.Id,
                ProductId = m.VariantSeller.ProductId,
                Title = m.VariantSeller.Product.Title,
                ImageUrl = m.VariantSeller.Product.ImageUrl,
                Url = m.VariantSeller.Product.Url,
                LastUpdatedDate = m.VariantSeller.Product.LastUpdatedDate,
                IsRead = m.IsRead,

                VariantSellerId = m.VariantSellerId,
                SellerName = m.VariantSeller.SellerName,
                VariantColorName = m.VariantSeller.ColorName, // Updated: Direct from Seller
                YourPrice = m.VariantSeller.CurrentPrice,
                IsWinningBuyBox = m.VariantSeller.IsBuyBoxWinner,

                // Updated: Search directly in Product.Sellers for the SAME ColorName
                CompetitorLowestPrice = m.VariantSeller.Product.Sellers
                    .Where(s => s.IsActive && s.Id != m.VariantSellerId && s.CurrentPrice > 0 && s.ColorName == m.VariantSeller.ColorName)
                    .Min(s => (decimal?)s.CurrentPrice) ?? 0,

                IsUndercut = m.VariantSeller.Product.Sellers
                    .Any(s => s.IsActive && s.Id != m.VariantSellerId && s.ColorName == m.VariantSeller.ColorName && s.CurrentPrice < m.VariantSeller.CurrentPrice),

                // Overall Product Context
                // Updated: Count distinct colors among all sellers for this product
                VariantsCount = m.VariantSeller.Product.Sellers.Select(s => s.ColorName).Distinct().Count(),
                SellersCount = m.VariantSeller.Product.Sellers.Count(s => s.IsActive)
            });

            // Sorting
            IOrderedQueryable<ClientSideProductDto> sortedQuery;

            switch (filter.SortType)
            {
                case ProductSortTypeEnum.OldestUpdate:
                    sortedQuery = projectedQuery.OrderBy(n => n.LastUpdatedDate);
                    break;
                case ProductSortTypeEnum.HighestPrice:
                    sortedQuery = projectedQuery.OrderByDescending(n => n.YourPrice);
                    break;
                case ProductSortTypeEnum.LowestPrice:
                    sortedQuery = projectedQuery.OrderBy(n => n.YourPrice);
                    break;
                case ProductSortTypeEnum.UnreadOnly:
                    sortedQuery = projectedQuery.OrderByDescending(n => n.LastUpdatedDate);
                    break;
                case ProductSortTypeEnum.NewestUpdate:
                default:
                    // Unread items first, then by newest update
                    sortedQuery = projectedQuery
                        .OrderBy(n => n.IsRead)
                        .ThenByDescending(n => n.LastUpdatedDate);
                    break;
            }

            // Paging is fully translatable to SQL OFFSET/FETCH
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
                .FirstOrDefaultAsync(n => n.UserId == userId && n.Id == id);
            if (userProduct is null)
                return Result.Failure(Messages.OperationFailedError);

            _context.UserProductMappings.Remove(userProduct);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> ClientSideSetProductAsReadForUser(int id)
        {
            return Result.Failure("This method is currently disabled for performance reasons. Please contact support for assistance.");
            /*            if (id < 1)
                            return Result.Failure(Messages.OperationFailedError);

                        var userId = _httpContextAccessor.HttpContext.User.GetUserId();
                        if (userId < 1)
                            return Result.Failure(Messages.OperationFailedError);

                        var rowsAffected = await _context.UserProductMappings
                            .Where(n => n.UserId == userId && n.ProductId == id)
                            .ExecuteUpdateAsync(s => s.SetProperty(upm => upm.IsRead, true));

                        if (rowsAffected == 0)
                            return Result.Failure(Messages.OperationFailedError);

                        return Result.Success();*/
        }

        public async Task<List<ProductMonitoringTargetDto>> GetAllProductsForMonitoringAsync(CancellationToken ct)
        {
            return new List<ProductMonitoringTargetDto>();
            /*            var now = DateTime.UtcNow;

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
                            .ToListAsync(ct);*/
        }

        public async Task<Result<List<ProductMonitoringTargetDto>>> GetProductBatchForMonitoringAsync(int page, int batchSize, CancellationToken ct)
        {
            return Result.Failure<List<ProductMonitoringTargetDto>>("This method is currently disabled for performance reasons. Please contact support for assistance.");
            /*          var now = DateTime.UtcNow;

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

                      return Result.Success(batch);*/
        }

        public async Task<Result<List<ClientSideUserChangeNotification>>> SyncProductAndDetectChangesAsync(
            ProductMonitoringTargetDto target,
            ClientSideScrapedProductDto scraped,
            CancellationToken ct)
        {
            return Result.Failure<List<ClientSideUserChangeNotification>>("This method is currently disabled for performance reasons. Please contact support for assistance.");
            /*            var strategy = _context.Database.CreateExecutionStrategy();

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

                                            // Build the description
                                            var changeDescription = BuildChangeDescription(
                                                colorName: scrapedVariant.ColorName,
                                                sellerName: scrapedVariant.SellerName,
                                                oldPrice: oldPrice,
                                                newPrice: scrapedVariant.Price,
                                                changePercent: changePercent,
                                                buyBoxChanged: buyBoxChanged,
                                                isBuyBoxWinner: scrapedVariant.IsBuyBoxWinner
                                            );

                                            dbSeller.PriceHistory.Add(new ProductPriceHistory
                                            {
                                                OldPrice = oldPrice,
                                                NewPrice = scrapedVariant.Price,
                                                ChangePercent = changePercent,
                                                BuyBoxChanged = buyBoxChanged,
                                                IsBuyBoxWinner = scrapedVariant.IsBuyBoxWinner,
                                                CreatedDate = now,
                                                LastUpdatedDate = now,
                                                ChangeDescription = changeDescription
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
                        });*/
        }

        #region SyncProduct Helper Methods
        private string BuildChangeDescription(
            string? colorName,
            string? sellerName,
            decimal oldPrice,
            decimal newPrice,
            decimal changePercent,
            bool buyBoxChanged,
            bool isBuyBoxWinner)
        {
            var parts = new List<string>();

            // Color info
            var colorInfo = !string.IsNullOrEmpty(colorName) ? $"({colorName})" : "";

            // Price change
            if (oldPrice != newPrice)
            {
                var direction = newPrice > oldPrice ? "افزایش" : "کاهش";
                var arrow = newPrice > oldPrice ? "↑" : "↓";

                parts.Add($"قیمت {colorInfo}: {FormatPrice(oldPrice)} ← {FormatPrice(newPrice)} ({arrow}{Math.Abs(changePercent):F1}% {direction})");
            }

            // BuyBox change
            if (buyBoxChanged)
            {
                var buyBoxStatus = isBuyBoxWinner
                    ? "🏆 بای‌باکس را به دست آورد"
                    : "❌ بای‌باکس را از دست داد";

                parts.Add(buyBoxStatus);
            }

            // Seller info
            if (!string.IsNullOrEmpty(sellerName))
            {
                parts.Add($"فروشنده: {sellerName}");
            }

            return string.Join(" | ", parts);
        }

        private string FormatPrice(decimal price)
        {
            if (price >= 1_000_000_000m)
                return $"{(price / 1_000_000_000m):F1} میلیارد";

            if (price >= 1_000_000m)
                return $"{(price / 1_000_000m):F1} میلیون";

            if (price >= 1_000m)
                return $"{price:N0}";

            return price.ToString("N0");
        }
        #endregion

        public async Task<Result<ClientSideProductSearchResultDto>> SearchProductForModalAsync(string url)
        {
            var userResult = await _userService.ClientSideCanUserAddProduct();
            if (userResult.IsFailure)
                return Result.Failure<ClientSideProductSearchResultDto>(userResult.Message!);

            if (string.IsNullOrWhiteSpace(url))
                return Result.Failure<ClientSideProductSearchResultDto>(Messages.InvalidDigiKalaProductUrlError);

            var extractResult = _digiKalaService.ExtractProductIdFromUrl(url);
            if (extractResult.IsFailure)
                return Result.Failure<ClientSideProductSearchResultDto>(Messages.InvalidDigiKalaProductUrlError);

            var digiKalaId = extractResult.Value;

            var existingProduct = await _context.Products
                .AsNoTracking()
                .Where(p => p.DigiKalaId == digiKalaId)
                .Select(p => new { p.Id, p.LastUpdatedDate })
                .FirstOrDefaultAsync();

            var fifteenMinutesAgo = DateTime.UtcNow.AddMinutes(-15);

            if (existingProduct == null)
            {
                var createResult = await CreateProductFromScrapeAsync(digiKalaId);
                if (createResult.IsFailure)
                    return Result.Failure<ClientSideProductSearchResultDto>(createResult.Message!);

                return await GetProductModalDataAsync(createResult.Value);
            }
            else if (existingProduct.LastUpdatedDate < fifteenMinutesAgo)
            {
                var updateResult = await UpdateProductFromScrapeAsync(existingProduct.Id, digiKalaId);
                if (updateResult.IsFailure)
                    return Result.Failure<ClientSideProductSearchResultDto>(updateResult.Message!);
            }

            return await GetProductModalDataAsync(existingProduct.Id);
        }

        public async Task<Result> AddProductToUserTrackingAsync(int productVariantSellerId)
        {
            var userResult = await _userService.ClientSideCanUserAddProduct();

            if (userResult.IsFailure)
                return Result.Failure(userResult.Message!);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var sellerExists = await _context.ProductVariantSellers
                .AnyAsync(s => s.Id == productVariantSellerId);

            if (!sellerExists)
                return Result.Failure(Messages.ProductVariantSellerNotFoundError);

            var alreadyTracking = await _context.UserProductMappings
                .AnyAsync(m => m.UserId == userId && m.VariantSellerId == productVariantSellerId);

            if (alreadyTracking)
                return Result.Failure(Messages.ThisProductAlreadyInYourWatchListError);

            var mapping = new UserProductMapping
            {
                UserId = userId,
                VariantSellerId = productVariantSellerId,
                IsRead = true
            };

            _context.UserProductMappings.Add(mapping);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        #region Private Helpers

        private async Task<Result<ClientSideProductSearchResultDto>> GetProductModalDataAsync(int productId)
        {
            var flatSellers = await _context.ProductVariantSellers
                .AsNoTracking()
                .Where(s => s.ProductId == productId && s.IsActive)
                .Select(s => new
                {
                    s.Id,
                    s.SellerName,
                    s.CurrentPrice,
                    s.IsBuyBoxWinner,
                    s.ColorName,
                    s.ColorHex,
                    s.DigiKalaVariantId
                })
                .ToListAsync();

            if (flatSellers == null || flatSellers.Count == 0)
                return Result.Failure<ClientSideProductSearchResultDto>(Messages.OperationFailedError);

            var productInfo = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == productId)
                .Select(p => new { p.Title, p.ImageUrl })
                .FirstAsync();

            // 2. Group them by Color in memory for the Modal UI
            var modalData = new ClientSideProductSearchResultDto
            {
                ProductId = productId,
                Title = productInfo.Title,
                ImageUrl = productInfo.ImageUrl,
                Options = flatSellers
                    .GroupBy(s => new { s.DigiKalaVariantId, s.ColorName, s.ColorHex })
                    .Select(g => new ClientSideProductOptionDto
                    {
                        ColorName = g.Key.ColorName,
                        ColorHex = g.Key.ColorHex,
                        Sellers = g.Select(s => new ClientSideProductSellerOptionDto
                        {
                            SellerId = s.Id,
                            SellerName = s.SellerName,
                            Price = s.CurrentPrice,
                            IsBuyBoxWinner = s.IsBuyBoxWinner
                        }).ToList()
                    }).ToList()
            };

            return Result.Success(modalData);
        }

        private async Task<Result<int>> CreateProductFromScrapeAsync(string digiKalaId)
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
                ImageUrl = scraped.ImageUrl,
                LastUpdatedDate = DateTime.UtcNow,
                Sellers = new List<ProductVariantSeller>()
            };

            foreach (var scrapedSeller in scraped.Sellers)
            {
                var sellerKey = scrapedSeller.SellerCode ?? scrapedSeller.SellerId.ToString();

                var seller = new ProductVariantSeller
                {
                    DigiKalaVariantId = scrapedSeller.DigiKalaVariantId,
                    ColorName = scrapedSeller.ColorName,
                    ColorHex = scrapedSeller.ColorHex,
                    DigiKalaSellerId = sellerKey,
                    SellerName = scrapedSeller.SellerName,
                    CurrentPrice = scrapedSeller.Price,
                    PreviousPrice = null,
                    IsBuyBoxWinner = scrapedSeller.IsBuyBoxWinner,
                    WasBuyBoxWinner = scrapedSeller.IsBuyBoxWinner,
                    SellerRating = scrapedSeller.SellerRating,
                    IsActive = scrapedSeller.Status == "marketable",
                    PriceHistory = new List<ProductPriceHistory>()
                };

                seller.PriceHistory.Add(new ProductPriceHistory
                {
                    OldPrice = scrapedSeller.Price,
                    NewPrice = scrapedSeller.Price,
                    ChangePercent = 0,
                    BuyBoxChanged = false,
                    IsBuyBoxWinner = scrapedSeller.IsBuyBoxWinner
                });

                product.Sellers.Add(seller);
            }

            _context.Products.Add(product);

            try
            {
                await _context.SaveChangesAsync();
                return Result.Success(product.Id);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    var existingId = await _context.Products
                        .AsNoTracking()
                        .Where(p => p.DigiKalaId == digiKalaId)
                        .Select(p => p.Id)
                        .FirstAsync();

                    return Result.Success(existingId);
                }
                throw;
            }
        }

        private async Task<Result> UpdateProductFromScrapeAsync(int productId, string digiKalaId)
        {
            var scrapeResult = await _digiKalaService.ScrapeFullProductAsync(digiKalaId);
            if (scrapeResult.IsFailure)
                return Result.Failure(scrapeResult.Message!);

            var scraped = scrapeResult.Value;

            var dbProduct = await _context.Products
                .Include(p => p.Sellers)
                    .ThenInclude(s => s.PriceHistory)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (dbProduct == null)
                return Result.Failure(Messages.InvalidDigiKalaProductUrlError);

            dbProduct.Title = scraped.Title;
            dbProduct.ImageUrl = scraped.ImageUrl;
            dbProduct.LastUpdatedDate = DateTime.UtcNow;


            var dbSellersDict = dbProduct.Sellers.ToDictionary(s => (s.DigiKalaVariantId, s.DigiKalaSellerId));
            var seenSellerKeys = new HashSet<(long, string)>();

            foreach (var scrapedSeller in scraped.Sellers)
            {
                var sellerKey = (scrapedSeller.DigiKalaVariantId, scrapedSeller.SellerCode ?? scrapedSeller.SellerId.ToString());
                seenSellerKeys.Add(sellerKey);

                if (!dbSellersDict.TryGetValue(sellerKey, out var dbSeller))
                {
                    // New seller entry
                    dbSeller = new ProductVariantSeller
                    {
                        DigiKalaVariantId = scrapedSeller.DigiKalaVariantId,
                        DigiKalaSellerId = sellerKey.Item2,
                        ColorName = scrapedSeller.ColorName,
                        ColorHex = scrapedSeller.ColorHex,
                        CurrentPrice = scrapedSeller.Price,
                        PreviousPrice = null,
                        IsBuyBoxWinner = scrapedSeller.IsBuyBoxWinner,
                        WasBuyBoxWinner = scrapedSeller.IsBuyBoxWinner,
                        SellerRating = scrapedSeller.SellerRating,
                        IsActive = scrapedSeller.Status == "marketable",
                        SellerName = scrapedSeller.SellerName,
                        PriceHistory = new List<ProductPriceHistory>
                {
                    new ProductPriceHistory
                    {
                        OldPrice = scrapedSeller.Price,
                        NewPrice = scrapedSeller.Price,
                        ChangePercent = 0,
                        BuyBoxChanged = false,
                        IsBuyBoxWinner = scrapedSeller.IsBuyBoxWinner
                    }
                }
                    };
                    dbProduct.Sellers.Add(dbSeller);
                }
                else
                {
                    // Existing seller entry - check for changes
                    bool priceChanged = dbSeller.CurrentPrice != scrapedSeller.Price;
                    bool buyBoxChanged = dbSeller.IsBuyBoxWinner != scrapedSeller.IsBuyBoxWinner;

                    if (priceChanged || buyBoxChanged)
                    {
                        var oldPrice = dbSeller.CurrentPrice;
                        var changePercent = oldPrice > 0 ? ((scrapedSeller.Price - oldPrice) / oldPrice) * 100 : 0;

                        dbSeller.PreviousPrice = oldPrice;
                        dbSeller.CurrentPrice = scrapedSeller.Price;
                        dbSeller.WasBuyBoxWinner = dbSeller.IsBuyBoxWinner;
                        dbSeller.IsBuyBoxWinner = scrapedSeller.IsBuyBoxWinner;

                        dbSeller.PriceHistory.Add(new ProductPriceHistory
                        {
                            OldPrice = oldPrice,
                            NewPrice = scrapedSeller.Price,
                            ChangePercent = changePercent,
                            BuyBoxChanged = buyBoxChanged,
                            IsBuyBoxWinner = dbSeller.IsBuyBoxWinner
                        });
                    }

                    // Update general info
                    dbSeller.SellerName = scrapedSeller.SellerName;
                    dbSeller.SellerRating = scrapedSeller.SellerRating;
                    dbSeller.IsActive = scrapedSeller.Status == "marketable";
                    dbSeller.ColorName = scrapedSeller.ColorName;
                    dbSeller.ColorHex = scrapedSeller.ColorHex;
                }
            }

            // Mark missing sellers as inactive
            foreach (var dbSeller in dbProduct.Sellers)
            {
                var sellerKey = (dbSeller.DigiKalaVariantId, dbSeller.DigiKalaSellerId);
                if (!seenSellerKeys.Contains(sellerKey))
                {
                    dbSeller.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }

        #endregion
    }
}