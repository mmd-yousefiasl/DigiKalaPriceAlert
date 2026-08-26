using Application.Dtos.DigiKala;
using Application.Dtos.Products.ClientSide;
using Application.Mediator;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.Products;
using Domain.Events.Products;
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
        private readonly IMediatorHandler _mediator;
        #endregion

        #region Ctor
        public ProductService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IDigiKalaService digiKalaService,
            IUserService userService,
            ILogger<IProductService> logger,
            IMediatorHandler mediator)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _digiKalaService = digiKalaService;
            _userService = userService;
            _logger = logger;
            _mediator = mediator;
        }
        #endregion

        public async Task<Result<ClientSideProductDetailsDto>> ClientSideGetProductDetails(int id)
        {
            if (id < 1)
                return Result.Failure<ClientSideProductDetailsDto>(
                    Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();

            if (userId < 1)
                return Result.Failure<ClientSideProductDetailsDto>(
                    Messages.OperationFailedError);

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            // ==========================================
            // 1. Get tracked variant/product context
            // ==========================================
            var trackingContext = await _context.UserProductMappings
                .AsNoTracking()
                .Where(m =>
                    m.Id == id &&
                    m.UserId == userId &&
                    !m.IsDeleted)
                .Select(m => new
                {
                    ProductId = m.VariantSeller.ProductId,
                    Title = m.VariantSeller.Product.Title,
                    DigiKalaId = m.VariantSeller.Product.DigiKalaId,
                    ImageUrl = m.VariantSeller.Product.ImageUrl,
                    Url = m.VariantSeller.Product.Url,
                    LastUpdatedDate = m.VariantSeller.Product.LastUpdatedDate,

                    // The COLOR is the competitor group. DigiKala's variant id
                    // is unique per color×seller combo, so it must NOT be used
                    // to find other sellers.
                    VariantColorName = m.VariantSeller.ColorName,
                    VariantColorHex = m.VariantSeller.ColorHex,

                    TrackedSellerId = m.VariantSellerId
                })
                .FirstOrDefaultAsync();

            if (trackingContext is null)
                return Result.Failure<ClientSideProductDetailsDto>(
                    Messages.OperationFailedError);

            // ==========================================
            // 2. Get all sellers of this COLOR (= the real competition arena)
            // ==========================================
            var sellers = await _context.ProductVariantSellers
                .AsNoTracking()
                .Where(s =>
                    s.ProductId == trackingContext.ProductId &&
                    s.ColorName == trackingContext.VariantColorName &&
                    !s.IsDeleted)
                .Select(s => new ClientSideVariantSellerDto
                {
                    Id = s.Id,
                    SellerName = s.SellerName,
                    CurrentPrice = s.CurrentPrice,
                    IsBuyBoxWinner = s.IsBuyBoxWinner,
                    IsUserTracked = s.Id == trackingContext.TrackedSellerId,
                    IsActive = s.IsActive,
                    SellerRating = s.SellerRating
                })
                .ToListAsync();

            if (sellers.Count == 0)
                return Result.Failure<ClientSideProductDetailsDto>(
                    Messages.OperationFailedError);

            // ==========================================
            // 3. Find current Buy Box winner
            // ==========================================
            var winnerId = sellers
                .FirstOrDefault(s => s.IsActive && s.IsBuyBoxWinner)
                ?.Id ?? 0;

            // ==========================================
            // 4. Get recent changes for this color group (consolidated records).
            //    Single indexed seek on (ProductId, ColorName, CreatedDate);
            //    items come from the jsonb column, bounded to the latest 50 records.
            // ==========================================
            var changeRecords = await _context.ProductVariantChanges
                .AsNoTracking()
                .Where(c =>
                    c.ProductId == trackingContext.ProductId &&
                    c.ColorName == trackingContext.VariantColorName &&
                    c.CreatedDate >= thirtyDaysAgo)
                .OrderByDescending(c => c.CreatedDate)
                .Take(50)
                .Select(c => new { c.CreatedDate, c.IsStockOut, c.Items })
                .ToListAsync();

            var history = changeRecords
                .Select(c => new ClientSideVariantChangeDto
                {
                    CreatedDate = c.CreatedDate,
                    IsStockOut = c.IsStockOut,
                    Items = c.Items.Select(i => new ClientSideVariantChangeItemDto
                    {
                        Type = i.Type,
                        SellerName = i.SellerName,
                        OldPrice = i.OldPrice,
                        NewPrice = i.NewPrice,
                        ChangePercent = i.ChangePercent
                    }).ToList()
                })
                .ToList();

            // ==========================================
            // 5. Calculate current market prices
            // ==========================================
            var activePricedSellers = sellers
                .Where(s => s.IsActive && s.CurrentPrice > 0)
                .ToList();

            var lowestCurrentPrice = activePricedSellers.Count > 0
                ? activePricedSellers.Min(s => s.CurrentPrice)
                : 0;

            var highestCurrentPrice = activePricedSellers.Count > 0
                ? activePricedSellers.Max(s => s.CurrentPrice)
                : 0;

            // ==========================================
            // 6. Build result
            // ==========================================
            var dto = new ClientSideProductDetailsDto
            {
                // Mapping id — the controller uses it to mark this tracking as read.
                Id = id,

                ProductId = trackingContext.ProductId,
                Title = trackingContext.Title,
                DigiKalaId = trackingContext.DigiKalaId,
                ImageUrl = trackingContext.ImageUrl,
                Url = trackingContext.Url,
                LastUpdatedDate = trackingContext.LastUpdatedDate,

                VariantColorName = trackingContext.VariantColorName,
                VariantColorHex = trackingContext.VariantColorHex,

                TotalActiveSellersInVariant =
                    sellers.Count(s => s.IsActive),

                LowestCurrentPrice = lowestCurrentPrice,
                HighestCurrentPrice = highestCurrentPrice,

                TrackedSeller = sellers
                    .FirstOrDefault(s => s.Id == trackingContext.TrackedSellerId),

                WinnerSeller = winnerId > 0
                    ? sellers.FirstOrDefault(s => s.Id == winnerId)
                    : null,

                Opponents = sellers
                    .Where(s =>
                        s.IsActive &&
                        s.Id != trackingContext.TrackedSellerId &&
                        s.Id != winnerId)
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
                VariantColorName = m.VariantSeller.ColorName,
                YourPrice = m.VariantSeller.CurrentPrice,
                IsWinningBuyBox = m.VariantSeller.IsBuyBoxWinner,

                // Competitors = other sellers of the SAME COLOR (matched by
                // ColorName — DigiKala's variant id is unique per seller combo)
                CompetitorLowestPrice = m.VariantSeller.Product.Sellers
                    .Where(s => s.IsActive && s.Id != m.VariantSellerId && s.CurrentPrice > 0 && s.ColorName == m.VariantSeller.ColorName)
                    .Min(s => (decimal?)s.CurrentPrice) ?? 0,

                IsUndercut = m.VariantSeller.Product.Sellers
                    .Any(s => s.IsActive && s.Id != m.VariantSellerId && s.ColorName == m.VariantSeller.ColorName && s.CurrentPrice < m.VariantSeller.CurrentPrice),

                // Overall Product Context: count distinct colors among all sellers for this product
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
            if (id < 1)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var rowsAffected = await _context.UserProductMappings
                .Where(n => n.UserId == userId && n.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(upm => upm.IsRead, true));

            if (rowsAffected == 0)
                return Result.Failure(Messages.OperationFailedError);

            return Result.Success();
        }

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

        public async Task<Result> AddProductToUserTrackingAsync(ClientSideAddTrackingDto dto)
        {
            var userResult = await _userService.ClientSideCanUserAddProduct();

            if (userResult.IsFailure)
                return Result.Failure(userResult.Message!);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var sellerExists = await _context.ProductVariantSellers
                .AnyAsync(s => s.Id == dto.SellerId);

            if (!sellerExists)
                return Result.Failure(Messages.ProductVariantSellerNotFoundError);

            var alreadyTracking = await _context.UserProductMappings
                .AnyAsync(m => m.UserId == userId && m.VariantSellerId == dto.SellerId);

            if (alreadyTracking)
                return Result.Failure(Messages.ThisProductAlreadyInYourWatchListError);

            var mapping = new UserProductMapping
            {
                UserId = userId,
                VariantSellerId = dto.SellerId,
                DigiKalaVariantId = dto.DigiKalaVariantId,
                IsRead = true
            };

            _context.UserProductMappings.Add(mapping);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        #region AddProductToUserTrackingAsync Private Helpers

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

            // Group flat sellers by COLOR in memory for the Modal UI.
            // (NOT by DigiKalaVariantId — every seller of a color has its own id.)
            var modalData = new ClientSideProductSearchResultDto
            {
                ProductId = productId,
                Title = productInfo.Title,
                ImageUrl = productInfo.ImageUrl,
                Options = flatSellers
                    .GroupBy(s => new { s.ColorName, s.ColorHex })
                    .Select(g => new ClientSideProductOptionDto
                    {
                        ColorName = g.Key.ColorName,
                        ColorHex = g.Key.ColorHex,
                        Sellers = g.Select(s => new ClientSideProductSellerOptionDto
                        {
                            SellerId = s.Id,
                            SellerName = s.SellerName,
                            DigiKalaVariantId = s.DigiKalaVariantId,
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
                product.Sellers.Add(new ProductVariantSeller
                {
                    DigiKalaVariantId = scrapedSeller.DigiKalaVariantId,
                    ColorName = scrapedSeller.ColorName,
                    ColorHex = scrapedSeller.ColorHex,
                    DigiKalaSellerId = scrapedSeller.SellerCode,
                    SellerName = scrapedSeller.SellerName,
                    CurrentPrice = scrapedSeller.Price,
                    IsBuyBoxWinner = scrapedSeller.IsBuyBoxWinner,
                    SellerRating = scrapedSeller.SellerRating,
                    IsActive = scrapedSeller.Status == "marketable"
                });
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

            var dbProduct = await _context.Products
                .Include(p => p.Sellers)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (dbProduct == null)
                return Result.Failure(Messages.InvalidDigiKalaProductUrlError);

            // Same reconciliation as the monitoring worker, so the change log stays
            // consistent no matter who triggered the refresh. Events are intentionally
            // discarded here — the worker remains the sole alert authority.
            ReconcileProduct(dbProduct, scrapeResult.Value, DateTime.UtcNow);

            await _context.SaveChangesAsync();
            return Result.Success();
        }

        #endregion

        public async Task ExecuteUpdateCycleAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Starting product update cycle...");

            // 1. Get products actually watched by at least one mapping
            var trackedProducts = await GetTrackedProductsAsync(ct);
            if (trackedProducts.Count == 0)
            {
                _logger.LogInformation("No tracked products to monitor.");
                return;
            }

            var random = new Random();
            var affectedColorGroups = 0;

            // 2. Scrape products sequentially with delay
            foreach (var trackedProduct in trackedProducts)
            {
                var scrapeResult = await _digiKalaService.ScrapeFullProductAsync(
                    trackedProduct.DigiKalaId,
                    ct);

                if (scrapeResult.Message == DigiKalaErrors.RateLimited)
                {
                    _logger.LogCritical(
                        "Digikala rate limit detected while scraping DKP {Id} — aborting cycle early.",
                        trackedProduct.DigiKalaId);
                    break;
                }

                if (!scrapeResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "Failed to scrape DKP {Id}: {Error}",
                        trackedProduct.DigiKalaId,
                        scrapeResult.Message);

                    await Task.Delay(random.Next(800, 1500), ct);
                    continue;
                }

                // 3. Diff scraped state against the DB; one event per affected color group
                var events = ReconcileProduct(trackedProduct, scrapeResult.Value, DateTime.UtcNow);

                // 4. Persist THIS product before moving on. A save that fails
                //    (constraint conflict, bad data) must never poison the rest
                //    of the batch or the next retries.
                try
                {
                    if (_context.ChangeTracker.HasChanges())
                        await _context.SaveChangesAsync(ct);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(
                        ex,
                        "Saving scrape results for DKP {Id} failed — reverting this product and continuing.",
                        trackedProduct.DigiKalaId);

                    RevertPendingChanges();

                    await Task.Delay(random.Next(800, 1500), ct);
                    continue;
                }

                affectedColorGroups += events.Count;

                // 5. Dispatch events only after their DB state is persisted
                foreach (var evt in events)
                {
                    await _mediator.PublishEvent(evt);
                }

                // Random delay 0.8s to 1.5s to prevent rate limiting
                await Task.Delay(random.Next(800, 1500), ct);
            }

            if (affectedColorGroups > 0)
                _logger.LogInformation(
                    "Update cycle finished — {Count} color groups changed.",
                    affectedColorGroups);
            else
                _logger.LogInformation("No market changes detected.");
        }

        #region ExecuteUpdateCycleAsync Private Helpers

        /// <summary>
        /// Only products followed by at least one non-deleted mapping.
        /// IsActive and seller-level IsDeleted are intentionally NOT filtered:
        /// deactivated (stocked-out) sellers must keep being scraped so they
        /// can reactivate, and soft-deleted rows must stay VISIBLE to the
        /// reconciliation — otherwise a returning seller would be inserted as
        /// a duplicate of its hidden corpse and violate the unique index
        /// (ProductId, DigiKalaVariantId, DigiKalaSellerId).
        /// </summary>
        private async Task<List<Product>> GetTrackedProductsAsync(CancellationToken ct)
        {
            return await _context.Products
                   .IgnoreQueryFilters()
                   .Where(p => !p.IsDeleted &&
                               p.Sellers.Any(s => s.Users.Any(m => !m.IsDeleted)))
                   .Include(p => p.Sellers)
                   .ToListAsync(ct);
        }

        /// <summary>
        /// Abandons every pending change in the tracker after a failed save so
        /// one poisoned product cannot keep breaking later products or retries.
        /// </summary>
        private void RevertPendingChanges()
        {
            foreach (var entry in _context.ChangeTracker.Entries()
                         .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                         .ToList())
            {
                entry.State = entry.State == EntityState.Added
                    ? EntityState.Detached
                    : EntityState.Unchanged;
            }
        }

        /// <summary>
        /// Shared reconciliation used by both the monitoring worker and the
        /// user-triggered refresh path: diffs scraped state against the DB,
        /// mutates seller rows, writes ONE consolidated change record per
        /// affected COLOR GROUP, and returns market-status events.
        ///
        /// IMPORTANT: groups are keyed by ColorName, because DigiKala's
        /// variant id is unique per color×seller combination — two sellers
        /// of the same color never share a DigiKalaVariantId.
        /// </summary>
        private List<ProductVariantMarketStatusUpdatedEvent> ReconcileProduct(
            Product dbProduct,
            ClientSideScrapedProductDto scraped,
            DateTime now)
        {
            var affectedColors = new HashSet<string>();
            var itemsByColor = new Dictionary<string, List<ProductVariantChangeItem>>();
            var foundSellerKeys = new HashSet<(long VariantId, string SellerId)>();
            bool metadataChanged = false;

            void AddItem(string colorName, ProductVariantChangeItem item)
            {
                if (!itemsByColor.TryGetValue(colorName, out var list))
                {
                    list = new List<ProductVariantChangeItem>();
                    itemsByColor[colorName] = list;
                }

                list.Add(item);
            }

            // Keep product metadata fresh
            if (dbProduct.Title != scraped.Title)
            {
                dbProduct.Title = scraped.Title;
                metadataChanged = true;
            }

            if (dbProduct.ImageUrl != scraped.ImageUrl)
            {
                dbProduct.ImageUrl = scraped.ImageUrl;
                metadataChanged = true;
            }

            // ==========================================
            // 1. PRODUCT LEVEL: Entire product is out of stock
            // ==========================================
            if (scraped.Sellers == null || scraped.Sellers.Count == 0)
            {
                if (dbProduct.IsActive)
                {
                    dbProduct.IsActive = false;
                    metadataChanged = true;
                }

                foreach (var dbSeller in dbProduct.Sellers.Where(s => s.IsActive))
                {
                    affectedColors.Add(dbSeller.ColorName);

                    dbSeller.IsActive = false;
                    dbSeller.LastUpdatedDate = now;

                    if (dbSeller.IsBuyBoxWinner)
                    {
                        dbSeller.IsBuyBoxWinner = false;

                        AddItem(dbSeller.ColorName, new ProductVariantChangeItem
                        {
                            Type = ChangeItemType.BuyBoxLost,
                            DigiKalaSellerId = dbSeller.DigiKalaSellerId,
                            SellerName = dbSeller.SellerName,
                            OldPrice = dbSeller.CurrentPrice
                        });
                    }

                    AddItem(dbSeller.ColorName, new ProductVariantChangeItem
                    {
                        Type = ChangeItemType.StockOut,
                        DigiKalaSellerId = dbSeller.DigiKalaSellerId,
                        SellerName = dbSeller.SellerName,
                        OldPrice = dbSeller.CurrentPrice
                    });
                }
            }
            else
            {
                // ==========================================
                // 2. PRODUCT LEVEL: Reactivate product
                // ==========================================
                if (!dbProduct.IsActive)
                {
                    dbProduct.IsActive = true;
                    metadataChanged = true;
                }

                // ==========================================
                // 3. SELLER LEVEL: Process scraped sellers
                // ==========================================
                foreach (var scrapedSeller in scraped.Sellers)
                {
                    foundSellerKeys.Add(
                        (scrapedSeller.DigiKalaVariantId, scrapedSeller.SellerCode));

                    var entity = dbProduct.Sellers.FirstOrDefault(s =>
                        s.DigiKalaVariantId == scrapedSeller.DigiKalaVariantId &&
                        s.DigiKalaSellerId == scrapedSeller.SellerCode);

                    // ==========================================
                    // BRAND NEW SELLER
                    // ==========================================
                    if (entity == null)
                    {
                        var newSeller = new ProductVariantSeller
                        {
                            ProductId = dbProduct.Id,
                            DigiKalaSellerId = scrapedSeller.SellerCode,
                            SellerName = scrapedSeller.SellerName,
                            CurrentPrice = scrapedSeller.Price,
                            IsBuyBoxWinner = scrapedSeller.IsBuyBoxWinner,
                            IsActive = true,
                            DigiKalaVariantId = scrapedSeller.DigiKalaVariantId,
                            ColorName = scrapedSeller.ColorName,
                            ColorHex = scrapedSeller.ColorHex,
                            SellerRating = scrapedSeller.SellerRating,
                            CreatedDate = now,
                            LastUpdatedDate = now
                        };

                        dbProduct.Sellers.Add(newSeller);

                        affectedColors.Add(scrapedSeller.ColorName);

                        AddItem(scrapedSeller.ColorName, new ProductVariantChangeItem
                        {
                            Type = ChangeItemType.SellerAdded,
                            DigiKalaSellerId = scrapedSeller.SellerCode,
                            SellerName = scrapedSeller.SellerName,
                            NewPrice = scrapedSeller.Price
                        });

                        if (scrapedSeller.IsBuyBoxWinner)
                        {
                            AddItem(scrapedSeller.ColorName, new ProductVariantChangeItem
                            {
                                Type = ChangeItemType.BuyBoxGained,
                                DigiKalaSellerId = scrapedSeller.SellerCode,
                                SellerName = scrapedSeller.SellerName,
                                NewPrice = scrapedSeller.Price
                            });
                        }

                        continue;
                    }

                    // ==========================================
                    // EXISTING SELLER
                    // ==========================================
                    bool wasInactive = !entity.IsActive;

                    bool priceChanged =
                        entity.CurrentPrice != scrapedSeller.Price;

                    bool buyBoxChanged =
                        entity.IsBuyBoxWinner != scrapedSeller.IsBuyBoxWinner;

                    // Keep mutable seller information up to date
                    entity.SellerName = scrapedSeller.SellerName;
                    entity.ColorName = scrapedSeller.ColorName;
                    entity.ColorHex = scrapedSeller.ColorHex;
                    entity.SellerRating = scrapedSeller.SellerRating;

                    if (!wasInactive && !priceChanged && !buyBoxChanged)
                        continue;

                    affectedColors.Add(entity.ColorName);

                    entity.LastUpdatedDate = now;

                    decimal oldPrice = entity.CurrentPrice;
                    decimal newPrice = scrapedSeller.Price;

                    decimal? changePercent =
                        oldPrice.CalculateChangePercent(newPrice);

                    if (wasInactive)
                    {
                        entity.IsActive = true;
                        entity.IsDeleted = false; // heal soft-deleted corpses on return

                        AddItem(entity.ColorName, new ProductVariantChangeItem
                        {
                            Type = ChangeItemType.SellerBackInStock,
                            DigiKalaSellerId = entity.DigiKalaSellerId,
                            SellerName = entity.SellerName,
                            NewPrice = newPrice
                        });
                    }

                    if (priceChanged)
                    {
                        entity.CurrentPrice = newPrice;

                        AddItem(entity.ColorName, new ProductVariantChangeItem
                        {
                            Type = ChangeItemType.PriceChanged,
                            DigiKalaSellerId = entity.DigiKalaSellerId,
                            SellerName = entity.SellerName,
                            OldPrice = oldPrice,
                            NewPrice = newPrice,
                            ChangePercent = changePercent
                        });
                    }

                    if (buyBoxChanged)
                    {
                        entity.IsBuyBoxWinner = scrapedSeller.IsBuyBoxWinner;

                        AddItem(entity.ColorName, new ProductVariantChangeItem
                        {
                            Type = scrapedSeller.IsBuyBoxWinner
                                ? ChangeItemType.BuyBoxGained
                                : ChangeItemType.BuyBoxLost,
                            DigiKalaSellerId = entity.DigiKalaSellerId,
                            SellerName = entity.SellerName,
                            NewPrice = scrapedSeller.Price
                        });
                    }
                }

                // ==========================================
                // 4. SELLER LEVEL: Missing / removed sellers
                // ==========================================
                var missingSellers = dbProduct.Sellers
                    .Where(s =>
                        s.IsActive &&
                        !foundSellerKeys.Contains(
                            (s.DigiKalaVariantId, s.DigiKalaSellerId)))
                    .ToList();

                foreach (var missingSeller in missingSellers)
                {
                    affectedColors.Add(missingSeller.ColorName);

                    missingSeller.IsActive = false;
                    missingSeller.LastUpdatedDate = now;

                    if (missingSeller.IsBuyBoxWinner)
                    {
                        missingSeller.IsBuyBoxWinner = false;

                        AddItem(missingSeller.ColorName, new ProductVariantChangeItem
                        {
                            Type = ChangeItemType.BuyBoxLost,
                            DigiKalaSellerId = missingSeller.DigiKalaSellerId,
                            SellerName = missingSeller.SellerName,
                            OldPrice = missingSeller.CurrentPrice
                        });
                    }

                    AddItem(missingSeller.ColorName, new ProductVariantChangeItem
                    {
                        Type = ChangeItemType.SellerRemoved,
                        DigiKalaSellerId = missingSeller.DigiKalaSellerId,
                        SellerName = missingSeller.SellerName,
                        OldPrice = missingSeller.CurrentPrice
                    });
                }
            }

            // ==========================================
            // 5. Stamp timestamp ONLY when something actually changed
            // ==========================================
            if (affectedColors.Count > 0 || metadataChanged)
            {
                dbProduct.LastUpdatedDate = now;
            }

            // ==========================================
            // 6. Write ONE consolidated change record per affected color group
            // ==========================================
            foreach (var (colorName, items) in itemsByColor)
            {
                _context.ProductVariantChanges.Add(new ProductVariantChange
                {
                    ProductId = dbProduct.Id,
                    ColorName = colorName,
                    IsStockOut = items.Any(i => i.Type == ChangeItemType.StockOut),
                    CreatedDate = now,
                    LastUpdatedDate = now,
                    Items = items
                });
            }

            // ==========================================
            // 7. BUILD MARKET STATUS EVENTS
            // ==========================================
            return BuildMarketStatusEvents(
                dbProduct,
                affectedColors,
                itemsByColor);
        }

        private static List<ProductVariantMarketStatusUpdatedEvent>
            BuildMarketStatusEvents(
                Product dbProduct,
                IEnumerable<string> affectedColors,
                Dictionary<string, List<ProductVariantChangeItem>> itemsByColor)
        {
            var events = new List<ProductVariantMarketStatusUpdatedEvent>();

            foreach (var colorName in affectedColors)
            {
                var colorSellers = dbProduct.Sellers
                    .Where(s => s.ColorName == colorName)
                    .ToList();

                var activeSellers = colorSellers
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.CurrentPrice)
                    .ToList();

                var changes = itemsByColor.TryGetValue(colorName, out var items)
                    ? items.Select(i => new VariantChangeItemInfo(
                        Type: i.Type,
                        DigiKalaSellerId: i.DigiKalaSellerId,
                        SellerName: i.SellerName,
                        OldPrice: i.OldPrice,
                        NewPrice: i.NewPrice,
                        ChangePercent: i.ChangePercent))
                        .ToList()
                    : [];

                var sellerStatuses = activeSellers
                    .Select(s => new SellerMarketStatus(
                        DigiKalaSellerId: s.DigiKalaSellerId,
                        SellerName: s.SellerName,
                        CurrentPrice: s.CurrentPrice,
                        IsBuyBoxWinner: s.IsBuyBoxWinner))
                    .ToList();

                events.Add(
                    new ProductVariantMarketStatusUpdatedEvent(
                        ProductId: dbProduct.Id,
                        ProductTitle: dbProduct.Title,
                        ProductUrl: dbProduct.Url,
                        VariantName: colorName,
                        Changes: changes,
                        Sellers: sellerStatuses,
                        IsAvailable:
                            activeSellers.Count > 0));
            }

            return events;
        }
        #endregion
    }
}
