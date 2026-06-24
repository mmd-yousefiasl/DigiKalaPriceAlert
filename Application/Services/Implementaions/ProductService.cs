using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Products;
using Domain.Models.Users;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class ProductService : IProductService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDigiKalaService _digiKalaService;
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public ProductService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IDigiKalaService digiKalaService,
            IUserService userService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _digiKalaService = digiKalaService;
            _userService = userService;
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

            IQueryable<Product> query = _context.Products;

            foreach (var item in conditions)
            {
                query = query.Where(item);
            }

            query = query.OrderByDescending(n => n.LastUpdatedDate);

            await filter.Paging(query.Select(n => new ClientSideProductDto()
            {
                Id = n.Id,
                ImageUrl = n.ImageUrl,
                Title = n.Title,
                LastUpdatedDate = n.LastUpdatedDate,
                NewPrice = n.NewPrice,
                Url = n.Url
            }));

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
    }
}