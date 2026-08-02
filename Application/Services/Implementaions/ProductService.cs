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

            // ── Query 1: Get context (Product Info + The exact Color the user is tracking) ──
            var trackingContext = await _context.UserProductMappings
                .AsNoTracking()
                .Where(m => m.Id == id && m.UserId == userId && !m.IsDeleted)
                .Select(m => new
                {
                    ProductId = m.VariantSeller.Variant.Product.Id,
                    Title = m.VariantSeller.Variant.Product.Title,
                    DigiKalaId = m.VariantSeller.Variant.Product.DigiKalaId,
                    ImageUrl = m.VariantSeller.Variant.Product.ImageUrl,
                    Url = m.VariantSeller.Variant.Product.Url,
                    LastUpdatedDate = m.VariantSeller.Variant.Product.LastUpdatedDate,

                    // We grab the ColorName to find ALL variants of this color!
                    VariantColorName = m.VariantSeller.Variant.ColorName,
                    VariantColorHex = m.VariantSeller.Variant.ColorHex,
                    TrackedSellerId = m.VariantSellerId
                })
                .FirstOrDefaultAsync();

            if (trackingContext is null)
                return Result.Failure<ClientSideProductDetailsDto>(Messages.OperationFailedError);

            // ── Query 2: Get ALL sellers for this Product AND this specific Color ──
            // This solves the issue of multiple VariantIds for the same color.
            var sellers = await _context.ProductVariantSellers
                .AsNoTracking()
                .Where(s => s.Variant.ProductId == trackingContext.ProductId
                         && s.Variant.ColorName == trackingContext.VariantColorName
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

            // Identify the winner explicitly
            var winnerId = sellers.FirstOrDefault(s => s.IsBuyBoxWinner)?.Id ?? 0;

            // ── Query 3: Get history for these sellers ──
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

            // ── Map to final DTO ──
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

                // Opponents = everyone EXCEPT the tracked seller and the winner
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
                query = query.Where(m => m.VariantSeller.Variant.Product.Title.Contains(filter.Search));
            }

            // Unread Filter
            if (filter.SortType == ProductSortTypeEnum.UnreadOnly)
            {
                query = query.Where(m => !m.IsRead);
            }

            var projectedQuery = query.Select(m => new ClientSideProductDto
            {
                Id = m.Id,
                ProductId = m.VariantSeller.Variant.ProductId,
                Title = m.VariantSeller.Variant.Product.Title,
                ImageUrl = m.VariantSeller.Variant.Product.ImageUrl,
                Url = m.VariantSeller.Variant.Product.Url,
                LastUpdatedDate = m.VariantSeller.Variant.Product.LastUpdatedDate,
                IsRead = m.IsRead,

                VariantSellerId = m.VariantSellerId,
                SellerName = m.VariantSeller.SellerName,
                VariantColorName = m.VariantSeller.Variant.ColorName,
                YourPrice = m.VariantSeller.CurrentPrice,
                IsWinningBuyBox = m.VariantSeller.IsBuyBoxWinner,

                // FIX: Search across ALL variants of this Product that have the SAME ColorName
                CompetitorLowestPrice = m.VariantSeller.Variant.Product.Variants
                    .Where(v => v.ColorName == m.VariantSeller.Variant.ColorName)
                    .SelectMany(v => v.Sellers)
                    .Where(s => s.IsActive && s.Id != m.VariantSellerId && s.CurrentPrice > 0)
                    .Min(s => (decimal?)s.CurrentPrice) ?? 0,

                IsUndercut = m.VariantSeller.Variant.Product.Variants
                    .Where(v => v.ColorName == m.VariantSeller.Variant.ColorName)
                    .SelectMany(v => v.Sellers)
                    .Any(s => s.IsActive && s.Id != m.VariantSellerId && s.CurrentPrice < m.VariantSeller.CurrentPrice),

                // Overall Product Context
                VariantsCount = m.VariantSeller.Variant.Product.Variants.Count(v => v.IsActive),
                SellersCount = m.VariantSeller.Variant.Product.Variants
                    .SelectMany(v => v.Sellers)
                    .Count(s => s.IsActive)
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
                return Result.Failure<ClientSideProductSearchResultDto>(userResult.Message!);

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
            var modalData = await _context.Products
                .AsNoTracking()
                .AsSplitQuery()
                .Where(p => p.Id == productId)
                .Select(p => new ClientSideProductSearchResultDto
                {
                    ProductId = p.Id,
                    Title = p.Title,
                    ImageUrl = p.ImageUrl,
                    Options = p.Variants
                        .Where(v => v.IsActive)
                        .Select(v => new ClientSideProductOptionDto
                        {
                            VariantId = v.Id,
                            ColorName = v.ColorName,
                            ColorHex = v.ColorHex,
                            Sellers = v.Sellers
                                .Where(s => s.IsActive)
                                .Select(s => new ClientSideProductSellerOptionDto
                                {
                                    SellerId = s.Id,
                                    SellerName = s.SellerName,
                                    Price = s.CurrentPrice,
                                    IsBuyBoxWinner = s.IsBuyBoxWinner
                                }).ToList()
                        }).ToList()
                }).FirstOrDefaultAsync();

            if (modalData == null)
                return Result.Failure<ClientSideProductSearchResultDto>(Messages.OperationFailedError);

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
                LastUpdatedDate = DateTime.UtcNow
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
                    DigiKalaSellerId = scrapedVariant.SellerCode ?? scrapedVariant.SellerId.ToString(),
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

            _context.Products.Add(product);

            try
            {
                await _context.SaveChangesAsync();
                return Result.Success(product.Id);
            }
            catch (DbUpdateException ex)
            {
                // Check if the inner exception is a Postgres unique constraint violation
                if (ex.InnerException is PostgresException pgEx &&
                    pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    // Race condition: Another process added this product milliseconds ago!
                    var existingId = await _context.Products
                        .AsNoTracking()
                        .Where(p => p.DigiKalaId == digiKalaId)
                        .Select(p => p.Id)
                        .FirstAsync();

                    return Result.Success(existingId);
                }

                // If it's a different database error, rethrow it
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
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Sellers)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (dbProduct == null) return Result.Failure(Messages.InvalidDigiKalaProductUrlError);


            dbProduct.Title = scraped.Title;
            dbProduct.ImageUrl = scraped.ImageUrl;
            dbProduct.LastUpdatedDate = DateTime.UtcNow;

            var scrapedVariantsGrouped = scraped.Variants
                .GroupBy(v => v.DigiKalaVariantId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var dbVariantsDict = dbProduct.Variants.ToDictionary(v => v.DigiKalaVariantId);

            foreach (var variantGroup in scrapedVariantsGrouped)
            {
                var digiVariantId = variantGroup.Key;
                var scrapedSellersList = variantGroup.Value;

                var firstScraped = scrapedSellersList.First();

                if (!dbVariantsDict.TryGetValue(digiVariantId, out var dbVariant))
                {
                    dbVariant = new ProductVariant { DigiKalaVariantId = digiVariantId };
                    dbProduct.Variants.Add(dbVariant);
                }

                dbVariant.ColorName = firstScraped.ColorName;
                dbVariant.ColorHex = firstScraped.ColorHex;
                dbVariant.IsActive = firstScraped.IsMarketable;

                var dbSellersDict = dbVariant.Sellers.ToDictionary(s => s.DigiKalaSellerId);
                var scrapedSellersDict = scrapedSellersList.ToDictionary(s => s.SellerCode ?? s.SellerId.ToString());

                foreach (var scrapedSeller in scrapedSellersList)
                {
                    var sellerKey = scrapedSeller.SellerCode ?? scrapedSeller.SellerId.ToString();

                    if (!dbSellersDict.TryGetValue(sellerKey, out var dbSeller))
                    {
                        dbSeller = new ProductVariantSeller { DigiKalaSellerId = sellerKey };
                        dbVariant.Sellers.Add(dbSeller);
                    }

                    bool priceChanged = dbSeller.CurrentPrice != scrapedSeller.Price;
                    bool buyBoxChanged = dbSeller.IsBuyBoxWinner != scrapedSeller.IsBuyBoxWinner;

                    if (priceChanged || buyBoxChanged)
                    {
                        dbSeller.PreviousPrice = dbSeller.CurrentPrice;
                        dbSeller.CurrentPrice = scrapedSeller.Price;
                        dbSeller.WasBuyBoxWinner = dbSeller.IsBuyBoxWinner;
                        dbSeller.IsBuyBoxWinner = scrapedSeller.IsBuyBoxWinner;

                        dbSeller.PriceHistory.Add(new ProductPriceHistory
                        {
                            OldPrice = dbSeller.PreviousPrice ?? 0,
                            NewPrice = dbSeller.CurrentPrice,
                            BuyBoxChanged = buyBoxChanged,
                            IsBuyBoxWinner = dbSeller.IsBuyBoxWinner
                        });
                    }

                    dbSeller.SellerName = scrapedSeller.SellerName;
                    dbSeller.SellerRating = scrapedSeller.SellerRating;
                    dbSeller.IsActive = scrapedSeller.IsMarketable;
                }

                foreach (var dbSeller in dbVariant.Sellers)
                {
                    if (!scrapedSellersDict.ContainsKey(dbSeller.DigiKalaSellerId))
                    {
                        dbSeller.IsActive = false;
                    }
                }
            }

            foreach (var dbVariant in dbProduct.Variants)
            {
                if (!scrapedVariantsGrouped.ContainsKey(dbVariant.DigiKalaVariantId))
                {
                    dbVariant.IsActive = false;
                    foreach (var seller in dbVariant.Sellers)
                    {
                        seller.IsActive = false;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }

        #endregion
    }
}