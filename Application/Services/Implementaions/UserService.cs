using Application.Dtos.SubscriptionPlans.BotSide;
using Application.Dtos.Users.BotSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Users;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class UserService : IUserService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public UserService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result> BotSideCanUserAddProduct(long chatId)
        {
            var currentDateTime = DateTime.UtcNow;

            var verificationData = await _context.Users
                .Where(u => u.ChatId == chatId)
                .Select(u => new
                {
                    u.SubscriptionPlanId,
                    u.SubscriptionEndDate,
                    PremiumMaxCount = u.SubscriptionPlan.MaximumProductCount,

                    CurrentProductCount = _context.UserProductMapping.Count(up => up.User.ChatId == chatId),

                    FreePlanMaxCount = _context.SubscriptionPlans
                        .Where(sp => sp.Id == 1)
                        .Select(sp => sp.MaximumProductCount)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (verificationData is null)
                return Result.Failure(Messages.OperationFailedError);

            int allowedMax;

            if (verificationData.SubscriptionPlanId == 1 || currentDateTime > verificationData.SubscriptionEndDate)
            {
                allowedMax = verificationData.FreePlanMaxCount > 0
                    ? verificationData.FreePlanMaxCount
                    : AppInfo.DefaultMaxProductsForUser;
            }
            else
            {
                allowedMax = verificationData.PremiumMaxCount;
            }

            if (verificationData.CurrentProductCount >= allowedMax)
                return Result.Failure(BotMessages.CantAddMoreProductsError(allowedMax));

            return Result.Success();
        }

        public async Task<Result> BotSideCreateNewUser(BotSideCreateNewUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.ChatId == dto.ChatId))
                return Result.Failure(BotMessages.UserAlreadyExistMessage);

            var mobileResult = dto.Mobile.NormalizeIranPhoneNumber();
            if (mobileResult.IsFailure)
                return Result.Failure(mobileResult.Message!);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Mobile == mobileResult.Value);
            if (user != null)
            {
                user.ChatId = dto.ChatId;
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Username = dto.UserName;

                _context.Users.Update(user);
            }
            else
            {
                user = new User()
                {
                    Mobile = mobileResult.Value,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Username = dto.UserName,
                    ChatId = dto.ChatId,
                    SubscriptionPlanId = 1,
                };

                _context.Users.Add(user);
            }

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> BotSideCreateNewUserProduct(BotSideCreateNewUserProductDto dto)
        {
            if (await _context.UserProductMapping.AnyAsync(up => up.User.ChatId == dto.ChatId && up.ProductId == dto.ProductId))
                return Result.Failure(BotMessages.ProductAlreadyInWatchListError);

            var userIdResult = await GetUserIdByChatId(dto.ChatId);
            if (userIdResult.IsFailure)
                return Result.Failure(userIdResult.Message!);

            var userProduct = new UserProductMapping
            {
                UserId = userIdResult.Value,
                ProductId = dto.ProductId,
            };
            _context.Add(userProduct);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> BotSideDoesUserExist(long chatId)
        {
            if (await _context.Users.AnyAsync(u => u.ChatId == chatId))
                return Result.Success();

            return Result.Failure(Messages.UserDoesNotExistError);
        }

        public async Task<Result<BotSideSubscriptionPlanDto>> GetCurrenUserSubscriptionPlanByChatId(long chatId)
        {
            var currentDateTime = DateTime.UtcNow;

            var dbData = await _context.Users
                .Where(u => u.ChatId == chatId)
                .Select(u => new
                {
                    u.SubscriptionPlanId,
                    u.SubscriptionEndDate,
                    CurrentPlanName = u.SubscriptionPlan.Name,
                    CurrentPlanMaxCount = u.SubscriptionPlan.MaximumProductCount,

                    FreePlan = _context.SubscriptionPlans
                        .Where(sp => sp.Id == 1)
                        .Select(sp => new { sp.Name, sp.MaximumProductCount })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (dbData is null)
                return Result.Failure<BotSideSubscriptionPlanDto>(Messages.OperationFailedError);

            var dto = new BotSideSubscriptionPlanDto();

            if (dbData.SubscriptionPlanId == 1)
            {
                dto.Name = "پلن رایگان";
                dto.MaximumProductCount = dbData.CurrentPlanMaxCount;
                dto.SubscriptionEndDate = null;
                dto.StatusDescription = "دائمی (بدون انقضا) ♾"; // اصلاح شد
            }
            else if (dbData.SubscriptionEndDate.HasValue && currentDateTime > dbData.SubscriptionEndDate.Value)
            {
                dto.Name = "پلن رایگان (انتقال اضطراری)";
                dto.MaximumProductCount = dbData.FreePlan?.MaximumProductCount ?? AppInfo.DefaultMaxProductsForUser;
                dto.SubscriptionEndDate = null;
                dto.StatusDescription = "⚠️ اشتراک ویژه شما منقضی شده و حساب به طرح پایه منتقل شده است."; // اصلاح شد
            }
            else
            {
                dto.Name = dbData.CurrentPlanName;
                dto.MaximumProductCount = dbData.CurrentPlanMaxCount;
                dto.SubscriptionEndDate = dbData.SubscriptionEndDate;

                var remainingDays = (dbData.SubscriptionEndDate!.Value - currentDateTime).Days;
                var displayDays = remainingDays < 0 ? 0 : remainingDays;
                dto.StatusDescription = $"فعال و معتبر (دارای زمان باقی‌مانده) ✅"; // اصلاح شد
            }

            return dto;
        }

        public async Task<Result<int>> GetUserIdByChatId(long chatId)
        {
            var userId = await _context.Users
                .Where(u => u.ChatId == chatId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
            if (userId < 1) return Result.Failure<int>(Messages.OperationFailedError);

            return userId;
        }

        public async Task<List<BotSideUserMonitoringDto>> GetUsersBatchForMonitoringAsync(int page, int batchSize, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Skip(page * batchSize)
                .Take(batchSize)
                .Select(u => new
                {
                    u.ChatId,
                    u.SubscriptionPlanId,
                    u.SubscriptionEndDate,
                    MaxAllowed = (u.SubscriptionPlanId == 1 || (u.SubscriptionEndDate.HasValue && now > u.SubscriptionEndDate.Value))
                        ? _context.SubscriptionPlans.Where(sp => sp.Id == 1).Select(sp => sp.MaximumProductCount).FirstOrDefault()
                        : u.SubscriptionPlan.MaximumProductCount,

                    AllProducts = u.Products
                        .Select(upm => new BotSideUserProductDetailsDto
                        {
                            ProductId = upm.ProductId,
                            DigiKalaId = upm.Product.DigiKalaId,
                            Title = upm.Product.Title,
                            CurrentPrice = upm.Product.NewPrice,
                            ImageFileName = upm.Product.ImageFileName
                        })
                })
                .ToListAsync(cancellationToken);

            return users.Select(u => new BotSideUserMonitoringDto
            {
                ChatId = u.ChatId,
                AllowedProducts = u.AllProducts.Take(u.MaxAllowed).ToList()
            }).ToList();
        }

        public async Task<List<BotSideUserTrackingProductDto>> GetUsersTrackingProduct(BotSideGetUsersTrackingProductDto dto)
        {
            var now = DateTime.UtcNow;

            return await _context.UserProductMapping
                .AsNoTracking()
                .Where(up => up.ProductId == dto.ProductId)
                .Select(up => new BotSideUserTrackingProductDto()
                {
                    Mobile = up.User.Mobile,
                    ChatId = up.User.ChatId,
                    IsPaidPlan = up.User.SubscriptionPlanId != 1 && (up.User.SubscriptionEndDate == null || up.User.SubscriptionEndDate > now)
                })
                .ToListAsync(dto.CancellationToken);
        }

        public async Task<Result<List<int>>> GetAllEligibleProductIdsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var rawUsersData = await _context.Users
                .AsNoTracking()
                .Select(u => new
                {
                    MaxAllowed = (u.SubscriptionPlanId == 1 || (u.SubscriptionEndDate.HasValue && now > u.SubscriptionEndDate.Value))
                        ? _context.SubscriptionPlans.Where(sp => sp.Id == 1).Select(sp => sp.MaximumProductCount).FirstOrDefault()
                        : u.SubscriptionPlan.MaximumProductCount,

                    ProductIds = u.Products.Select(upm => upm.ProductId)
                })
                .ToListAsync(cancellationToken);

            var allUniqueEligibleProductIds = rawUsersData
                .SelectMany(u => u.ProductIds.Take(u.MaxAllowed))
                .Distinct()
                .ToList();
            if (!allUniqueEligibleProductIds.Any())
                return Result.Failure<List<int>>(Messages.OperationFailedError);

            return allUniqueEligibleProductIds;
        }
    }
}