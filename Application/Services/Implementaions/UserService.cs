using Application.Dtos.BalanceTransaction.AdminSide;
using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Dtos.SendSms.ClientSide;
using Application.Dtos.Users.AdminSide;
using Application.Dtos.Users.ClientSide;
using Application.Dtos.UserValidation;
using Application.Mediator;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.SubscriptionPlans;
using Domain.Enums.Users;
using Domain.Events.Users;
using Domain.Models.Users;
using Domain.Shared;
using Domain.Statics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Cryptography;

namespace Application.Services.Implementaions
{
    public class UserService : IUserService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISendSmsService _sendSmsService;
        private readonly IMediatorHandler _mediator;
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public UserService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ISendSmsService sendSmsService,
            IMediatorHandler mediator,
            ICacheManager cacheManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _sendSmsService = sendSmsService;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }
        #endregion

        public async Task<Result> IsSuperAdmin(int userId)
        {
            var key = string.Format(CacheKeys.IsSuperAdminCacheKey, userId);

            var result = await _cacheManager.GetAsync(key, async () =>
            {
                return await _context.Users
                .AsNoTracking()
                .AnyAsync(user => user.Id == userId && user.UserRoleMapping.Any(mapping => mapping.RoleId == 1));
            }, TimeSpan.FromMinutes(SiteTools.UserPermissionCacheExpirationTimeInMinutes));

            if (result)
            {
                return Result.Success();
            }
            else
            {
                return Result.Failure(Messages.OperationFailedError);
            }
        }

        public async Task<Result> CheckUserRoleIds(int userId, List<int> roleIds)
        {
            if (!roleIds?.Any() ?? true)
                return Result.Failure(Messages.OperationFailedError);

            var key = string.Format(CacheKeys.UserRoleIdsCacheKey, userId);
            var userRoleIds = await _cacheManager.GetAsync(key, async () =>
            {
                return await _context.UserRoleMapping
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .Select(n => n.RoleId)
                .ToListAsync();
            }, TimeSpan.FromMinutes(SiteTools.UserPermissionCacheExpirationTimeInMinutes));

            if (!userRoleIds?.Any() ?? true)
                return Result.Failure(Messages.OperationFailedError);

            var result = userRoleIds!.Any(roleId => roleIds!.Contains(roleId));
            if (result)
            {
                return Result.Success();
            }
            else
            {
                return Result.Failure(Messages.OperationFailedError);
            }
        }

        public async Task<Result<DateTime>> SendUserLoginCode(ClientSideLoginDto model)
        {
            if (model is null)
                return Result.Failure<DateTime>(Messages.OperationFailedError);

            User? loginUser = await _context.Users.Where(n => n.Mobile == model.Mobile).FirstOrDefaultAsync();
            if (loginUser is null)
            {
                loginUser = new User();

                loginUser.Mobile = model.Mobile;
                loginUser.IsUserBanned = false;

                _context.Users.Add(loginUser);

                try
                {
                    await _context.SaveChangesAsync();

                    UserCreatedEvent UserCreatedEvent = new UserCreatedEvent(loginUser.Mobile);
                    await _mediator.PublishEvent(UserCreatedEvent);
                }
                catch (DbUpdateException ex)
                    when (ex.InnerException is PostgresException pg && pg.SqlState == "23505" && pg.ConstraintName == "IX_Users_Mobile")
                {
                    _context.Entry(loginUser).State = EntityState.Detached;
                    loginUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Mobile == model.Mobile);

                    if (loginUser is null)
                        return Result.Failure<DateTime>(Messages.OperationFailedError);
                }
            }
            else
            {
                if (loginUser.IsUserBanned)
                    return Result.Failure<DateTime>(Messages.OperationFailedError);

                if (loginUser.LockoutEnd.HasValue && loginUser.LockoutEnd.Value > DateTime.UtcNow)
                {
                    var remainingMins = Math.Ceiling((loginUser.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                    return Result.Failure<DateTime>($"بیش از حد مجاز تلاش کرده‌اید. لطفا {remainingMins} دقیقه دیگر مجددا تلاش کنید.");
                }
                else
                {
                    loginUser.AccessFailedCount = 0;
                    loginUser.LockoutEnd = null;
                    await _context.SaveChangesAsync();
                }
            }

            var validationResult = await GenerateUserValidationCode(model.Mobile);
            if (validationResult.IsFailure)
                return Result.Failure<DateTime>(validationResult.Message!);

            var validation = validationResult.Value;

            if (validation.HasActiveCode == false && !string.IsNullOrWhiteSpace(validation.Code))
            {
                var smsResult = await _sendSmsService.ClientSideSendLoginCode(new ClientSideSendLoginCodeDto()
                {
                    Code = validation.Code,
                    Mobile = loginUser.Mobile
                });
            }

            return validation.CreatedDate;
        }

        public async Task<Result<ClientSideUserDto>> VerifyLoginCode(ClientSideLoginCodeDto model)
        {
            if (model is null) return Result.Failure<ClientSideUserDto>(Messages.OperationFailedError);

            string loginCode = string.Join("", model.LoginCode);

            var validationResult = await ValidateUserLoginCode(model.Mobile, loginCode);
            if (validationResult.IsFailure)
                return Result.Failure<ClientSideUserDto>(validationResult.Message!);

            var user = await _context.Users.Where(n => n.Mobile == model.Mobile).Select(n => new ClientSideUserDto()
            {
                Id = n.Id,
                IsBanned = n.IsUserBanned,
                Mobile = n.Mobile,
            }).AsNoTracking().FirstOrDefaultAsync();
            if (user is null) return Result.Failure<ClientSideUserDto>(Messages.OperationFailedError);

            return user;
        }

        public async Task<Result<AdminSideFilterUsersDto>> AdminSideFilterUsers(AdminSideFilterUsersDto filter)
        {
            if (filter is null) return Result.Failure<AdminSideFilterUsersDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<User>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Mobile.Contains(filter.Search) || n.FirstName.Contains(filter.Search) ||
                n.LastName.Contains(filter.Search));
            }

            if (filter.IsUserBanned is not null)
            {
                conditions.Add(n => n.IsUserBanned == filter.IsUserBanned);
            }

            IQueryable<User> query = _context.Users;

            query = query.OrderByDescending(n => n.Id);

            foreach (var condition in conditions ?? new FilterConditions<User>())
            {
                query = query.Where(condition);
            }

            await filter.Paging(query.Select(n => new AdminSideUserDto()
            {
                Id = n.Id,
                Mobile = n.Mobile,
                FirstName = n.FirstName,
                LastName = n.LastName,
                IsUserBanned = n.IsUserBanned
            }));

            return filter;
        }

        public async Task<Result> AdminSideCreateNewUser(AdminSideCreateNewUserDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            if (await _context.Users.AnyAsync(n => n.Mobile == model.Mobile))
                return Result.Failure(Messages.ThisPhoneNumberAlreadyRegisteredError);

            User user = new User()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsUserBanned = model.IsUserBanned,
                Mobile = model.Mobile,
                UserRoleMapping = new List<UserRoleMapping>(),
            };

            if (model.RoleIds?.Any() ?? false)
            {
                foreach (var item in model.RoleIds ?? new List<int>())
                {
                    user.UserRoleMapping.Add(new UserRoleMapping()
                    {
                        UserId = user.Id,
                        RoleId = item,
                    });
                }
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> AdminSideDeleteUser(int id)
        {
            if (id < 1) return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.FindAsync(id);
            if (user is null) return Result.Failure(Messages.OperationFailedError);

            user.IsDeleted = true;
            user.LastUpdatedDate = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            UserDeletedEvent UserDeletedEvent = new UserDeletedEvent(user.Id);

            await _mediator.PublishEvent(UserDeletedEvent);

            return Result.Success();
        }

        public async Task<Result<AdminSideUpdateUserDto>> AdminSideGetUserForUpdate(int id)
        {
            if (id < 1) return Result.Failure<AdminSideUpdateUserDto>(Messages.OperationFailedError);

            var user = await _context.Users.Where(n => n.Id == id).Select(n => new AdminSideUpdateUserDto()
            {
                Id = n.Id,
                FirstName = n.FirstName,
                LastName = n.LastName,
                IsUserBanned = n.IsUserBanned,
                Mobile = n.Mobile,
                RoleIds = n.UserRoleMapping.Select(n => n.RoleId).ToList()
            }).AsNoTracking().FirstOrDefaultAsync();
            if (user is null) return Result.Failure<AdminSideUpdateUserDto>(Messages.OperationFailedError);

            return user;
        }

        public async Task<Result> AdminSideUpdateUser(AdminSideUpdateUserDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.Include(n => n.UserRoleMapping).FirstOrDefaultAsync(n => n.Id == model.Id);
            if (user is null) return Result.Failure(Messages.OperationFailedError);

            user.LastUpdatedDate = DateTime.UtcNow;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.IsUserBanned = model.IsUserBanned;
            user.Mobile = model.Mobile;

            if (await _context.Users.AnyAsync(n => n.Id != model.Id && n.Mobile == model.Mobile))
                return Result.Failure(Messages.ThisPhoneNumberAlreadyRegisteredError);

            var removedRoles = user.UserRoleMapping.Select(n => n.RoleId).Except(model.RoleIds ?? new List<int>()).ToList();

            foreach (var item in removedRoles ?? new List<int>())
            {
                user.UserRoleMapping.Remove(user.UserRoleMapping.First(n => n.RoleId == item));
            }
            var newRoles = model.RoleIds?.Except(user.UserRoleMapping.Select(n => n.RoleId)).ToList();
            foreach (var item in newRoles ?? new List<int>())
            {
                user.UserRoleMapping.Add(new UserRoleMapping
                {
                    RoleId = item
                });
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            UserUpdatedEvent UserUpdatedEvent = new UserUpdatedEvent(user.Id);

            await _mediator.PublishEvent(UserUpdatedEvent);

            return Result.Success();
        }

        public async Task<Result<AdminSideUserDetailsDto>> AdminSideGetUserDetails(int id)
        {
            if (id < 1)
                return Result.Failure<AdminSideUserDetailsDto>(Messages.OperationFailedError);

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new AdminSideUserDetailsDto()
                {
                    Id = u.Id,
                    IsUserBanned = u.IsUserBanned,
                    DaysToNotificationSubscriptionEnd = u.NotificationSubscriptionEndDate.DaysRemaining(),
                    DaysToAutoPriceSubscriptionEnd = u.AutoPriceSubscriptionEndDate.DaysRemaining(),
                    Balance = u.Balance,
                }).FirstOrDefaultAsync();
            if (user is null)
                return Result.Failure<AdminSideUserDetailsDto>(Messages.OperationFailedError);

            return user;
        }

        public async Task<decimal> ClientSideGetCurrentUserBalance()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return 0;

            var userBalance = await _context.Users.Where(n => n.Id == userId)
                .Select(n => n.Balance)
                .FirstOrDefaultAsync();
            if (userBalance < 0)
                return 0;

            return userBalance;
        }

        public async Task<Result> ClientSideWithdrawUserOrder(ClientSideWithdrawUserOrderDto dto)
        {
            if (dto.Amount < 0) return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1) return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.Where(n => n.Id == userId).Include(n => n.Transactions).FirstOrDefaultAsync();
            if (user is null) return Result.Failure(Messages.OperationFailedError);

            if (user.Balance < dto.Amount) return Result.Failure(Messages.InSufficentBalanceError);

            user.Balance -= dto.Amount;
            user.Transactions.Add(new BalanceTransaction()
            {
                IsSuccess = true,
                Amount = dto.Amount,
                TransId = null,
                Type = BalanceTransactionTypeEnum.Debit,
                Description = $"کاهش موجودی کیف پول جهت تسویه حساب سفارش {dto.InvoiceId}"
            });

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> ClientSideAddToUserSubscription(int durationInDays, UsedFreePlanTypeEnum type)
        {
            if (durationInDays < 1)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                return Result.Failure(Messages.OperationFailedError);

            if (type == UsedFreePlanTypeEnum.Notification)
            {
                if (user.NotificationSubscriptionEndDate == null || user.NotificationSubscriptionEndDate <= DateTime.UtcNow)
                {
                    user.NotificationSubscriptionEndDate = DateTime.UtcNow.AddDays(durationInDays);
                }
                else
                {
                    user.NotificationSubscriptionEndDate = user.NotificationSubscriptionEndDate.Value.AddDays(durationInDays);
                }
            }
            else
            {
                if (user.AutoPriceSubscriptionEndDate == null || user.AutoPriceSubscriptionEndDate <= DateTime.UtcNow)
                {
                    user.AutoPriceSubscriptionEndDate = DateTime.UtcNow.AddDays(durationInDays);
                }
                else
                {
                    user.AutoPriceSubscriptionEndDate = user.AutoPriceSubscriptionEndDate.Value.AddDays(durationInDays);
                }
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> ClientSideCanUserAddProduct()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            if (await _context.UserProductMappings.CountAsync(n => n.UserId == userId) >= SiteTools.MaximumProductCount)
                return Result.Failure(Messages.UserReachedMaximumProductsError);

            return Result.Success();
        }

        public async Task<Result> ClientSideIsUserHaveSubscription(int userId)
        {
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var currentDate = DateTime.UtcNow;

            if (await _context.Users.AnyAsync(n => n.Id == userId && n.NotificationSubscriptionEndDate > currentDate))
                return Result.Success();

            return Result.Failure(Messages.OperationFailedError);
        }

        public async Task<Result<OtpGenerationResultDto>> GenerateUserValidationCode(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return Result.Failure<OtpGenerationResultDto>(Messages.OperationFailedError);

            var existingValidations = await _context.UserValidation
                .Where(uv => uv.Mobile == mobile)
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();

            var latestValidation = existingValidations.FirstOrDefault();

            if (latestValidation != null && DateTime.UtcNow - latestValidation.CreatedDate < TimeSpan.FromMinutes(2))
            {
                return Result.Success(new OtpGenerationResultDto
                {
                    Code = null,
                    CreatedDate = latestValidation.CreatedDate,
                    HasActiveCode = true
                });
            }

            if (existingValidations.Any())
            {
                _context.UserValidation.RemoveRange(existingValidations);
            }

            var code = Generate.UserLoginCode();
            var hashedCode = code.HashPassword(out string salt);
            var nowUtc = DateTime.UtcNow;

            var validation = new UserValidation
            {
                Code = hashedCode,
                Salt = salt,
                Mobile = mobile,
                CreatedDate = nowUtc,
            };

            _context.UserValidation.Add(validation);
            await _context.SaveChangesAsync();

            return Result.Success(new OtpGenerationResultDto
            {
                Code = code,
                CreatedDate = nowUtc,
                HasActiveCode = false
            });
        }

        public async Task<Result> ValidateUserLoginCode(string mobile, string code)
        {
            if (string.IsNullOrWhiteSpace(mobile) || string.IsNullOrWhiteSpace(code))
                return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.Where(u => u.Mobile == mobile).FirstOrDefaultAsync();
            if (user is null) return Result.Failure(Messages.OperationFailedError);

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var remainingMins = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                return Result.Failure($"حساب شما قفل شده است. {remainingMins} دقیقه دیگر تلاش کنید.");
            }

            var validation = await _context.UserValidation.Where(uv => uv.Mobile == mobile).FirstOrDefaultAsync();
            if (validation is null || validation.CreatedDate.AddMinutes(2) < DateTime.UtcNow)
            {
                return Result.Failure(Messages.LoginCodeExpiredError);
            }

            if (!code.VerifyPassword(validation.Salt, validation.Code))
            {
                user.AccessFailedCount++;

                if (user.AccessFailedCount >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddHours(2);
                    user.AccessFailedCount = 0;
                }

                await _context.SaveChangesAsync();
                return Result.Failure(Messages.InvalidLoginCodeError);
            }

            user.AccessFailedCount = 0;
            user.LockoutEnd = null;

            _context.UserValidation.Remove(validation);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> AdminSideCreateNewTransaction(AdminSideCreateNewBalanceTransactionDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Set<User>().FindAsync(model.UserId);
            if (user is null) return Result.Failure(Messages.OperationFailedError);

            // Create the transaction
            var transaction = new BalanceTransaction()
            {
                Amount = model.Amount,
                Type = model.Type,
                Description = model.Description,
                UserId = model.UserId,
                IsSuccess = true,
            };

            // Update user's balance
            if (model.Type == BalanceTransactionTypeEnum.Credit) user.Balance += model.Amount;
            else user.Balance -= model.Amount;

            if (user.Balance < 0) return Result.Failure(Messages.UserBalnaceCantBeLessThanZeroError);

            // Save both transaction and updated user
            _context.BalanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<AdminSideFilterBalanceTransactionsDto>> AdminSideFilterTransactions(AdminSideFilterBalanceTransactionsDto filter)
        {
            if (filter is null)
                return Result.Failure<AdminSideFilterBalanceTransactionsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<BalanceTransaction>();

            conditions.Add(n => n.UserId == filter.UserId);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.TransId.Contains(filter.Search) ||
                n.Description.Contains(filter.Search));
            }

            if (filter.Type is not null)
            {
                conditions.Add(n => n.Type == filter.Type);
            }

            if (filter.IsSuccess is not null)
            {
                conditions.Add(n => n.IsSuccess == filter.IsSuccess);
            }

            IQueryable<BalanceTransaction> query = _context.BalanceTransactions;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<BalanceTransaction>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideBalanceTransactionDto()
            {
                Id = n.Id,
                Amount = n.Amount,
                Description = n.Description,
                IsSuccess = n.IsSuccess,
                TransId = n.TransId,
                Type = n.Type,
                CreatedDate = n.CreatedDate,
            }));

            return filter;
        }

        public async Task<Result<ClientSideFilterBalanceTransactionsDto>> ClientSideFilterTransactions(ClientSideFilterBalanceTransactionsDto filter)
        {

            if (filter is null)
                return Result.Failure<ClientSideFilterBalanceTransactionsDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideFilterBalanceTransactionsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<BalanceTransaction>();

            conditions.Add(n => n.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.TransId.Contains(filter.Search) ||
                n.Description.Contains(filter.Search));
            }

            if (filter.Type is not null)
            {
                conditions.Add(n => n.Type == filter.Type);
            }

            if (filter.IsSuccess is not null)
            {
                conditions.Add(n => n.IsSuccess == filter.IsSuccess);
            }

            IQueryable<BalanceTransaction> query = _context.BalanceTransactions;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<BalanceTransaction>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new ClientSideBalanceTransactionDto()
            {
                Amount = n.Amount,
                Description = n.Description,
                IsSuccess = n.IsSuccess,
                TransId = n.TransId,
                Type = n.Type,
                CreatedDate = n.CreatedDate,
            }));

            return filter;
        }

        public async Task<Result> ClientSideAddUnVerifiedTransaction(ClientSideCreateNewTransactionDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1) return Result.Failure(Messages.OperationFailedError);

            var transaction = new BalanceTransaction()
            {
                Amount = model.Amount,
                Type = model.Type,
                Description = model.Description,
                IsSuccess = model.IsSuccess,
                TransId = model.TransId,
                UserId = userId
            };

            _context.BalanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> ClientSideVerifyTransaction(string transId)
        {
            var transaction = await _context.BalanceTransactions
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.TransId == transId);

            if (transaction is null)
                return Result.Failure(Messages.OperationFailedError);

            if (transaction.IsSuccess)
                return Result.Success(Messages.TransactionAlreadyVerified);

            transaction.IsSuccess = true;
            transaction.User.Balance += transaction.Amount;

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> ClientSideIsUserProfileCompleted(int userId)
        {
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                return Result.Failure(Messages.OperationFailedError);

            if (user.BaleChatId is null)
                return Result.Failure(Messages.TelegramBotNotConnectedError);

            return Result.Success();
        }

        public async Task<Result> ClientSideIsUserProfileCompleted()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();

            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                return Result.Failure(Messages.OperationFailedError);

            if (user.BaleChatId is null)
                return Result.Failure(Messages.TelegramBotNotConnectedError);

            return Result.Success();
        }

        public async Task<Result> ClientSideUpdateUserProfile(ClientSideUpdateProfileDto dto)
        {
            if (dto is null)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                return Result.Failure(Messages.OperationFailedError);

            if (user.BaleChatId is null)
                return Result.Failure(Messages.TelegramBotNotConnectedError);

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Success(Messages.YourProfileUpdatedSuccessfully);
        }

        public async Task<List<ClientSideUserToNotifyDto>> GetUserContactsAsync(List<int> userIds, CancellationToken ct)
        {
            if (userIds is null || userIds.Count == 0)
                return new List<ClientSideUserToNotifyDto>();

            return await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id) && !u.IsUserBanned)
                .Select(u => new ClientSideUserToNotifyDto
                {
                    UserId = u.Id,
                    Mobile = u.Mobile,
                    FirstName = u.FirstName ?? "کاربر"
                })
                .ToListAsync(ct);
        }

        public async Task<Result<ClientSideUpdateProfileDto>> ClientSideGetUserProfileForUpdate()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideUpdateProfileDto>(Messages.OperationFailedError);

            var user = await _context.Users
                .AsNoTracking()
                .Where(n => n.Id == userId)
                .Select(n => new ClientSideUpdateProfileDto()
                {
                    FirstName = n.FirstName,
                    LastName = n.LastName
                }).FirstOrDefaultAsync();
            if (user is null)
                return Result.Failure<ClientSideUpdateProfileDto>(Messages.OperationFailedError);

            return user;
        }

        public async Task<Result<string>> ClientSideConnectUserToBale()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();

            if (userId < 1)
                return Result.Failure<string>(Messages.OperationFailedError);

            // Remove any existing validation code for this user
            var existingValidations = await _context.BaleValidation
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (existingValidations.Any())
            {
                _context.BaleValidation.RemoveRange(existingValidations);
            }

            // Generate a secure, URL-safe random code
            var randomBytes = RandomNumberGenerator.GetBytes(32);

            var code = Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            var validation = new BaleValidation
            {
                UserId = userId,
                Code = code
            };

            await _context.BaleValidation.AddAsync(validation);
            await _context.SaveChangesAsync();

            var baleUrl = $"{SiteTools.BaleBaseUrl}{SiteTools.BaleBotUsername}?start={code}";

            return baleUrl;
        }

        public async Task<Result> ClientSideAuthenticateBaleUserAsync(string authKey, long chatId)
        {
            if (string.IsNullOrWhiteSpace(authKey) || chatId <= 0)
                return Result.Failure(Messages.InvalidDataError);

            var minimumCreatedDate = DateTime.UtcNow.AddMinutes(-20);

            var authResult = await _context.BaleValidation
                .FirstOrDefaultAsync(n =>
                    n.Code == authKey &&
                    n.CreatedDate >= minimumCreatedDate);

            if (authResult is null)
                return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.FindAsync(authResult.UserId);

            if (user is null)
                return Result.Failure(Messages.OperationFailedError);

            user.BaleChatId = chatId;
            user.LastUpdatedDate = DateTime.UtcNow;

            _context.BaleValidation.Remove(authResult);

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<long>> ClientSideGetCurrentUserChatId()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<long>(Messages.OperationFailedError);

            var chatId = await _context.Users
                .AsNoTracking()
                .Where(n => n.Id == userId)
                .Select(n => n.BaleChatId)
                .FirstOrDefaultAsync();

            if (chatId is null || chatId < 1)
                return Result.Failure<long>(Messages.TelegramBotNotConnectedError);

            return chatId;
        }

        public async Task<Result<ClientSideUserSubscriptionsDto>> ClientSideCurrentUserSubscriptions()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideUserSubscriptionsDto>(Messages.OperationFailedError);

            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                return Result.Failure<ClientSideUserSubscriptionsDto>(Messages.OperationFailedError);

            return new ClientSideUserSubscriptionsDto()
            {
                DaysToAutoPriceSubscriptionEnd = user.AutoPriceSubscriptionEndDate.DaysRemaining(),
                DaysToNotificationSubscriptionEnd = user.NotificationSubscriptionEndDate.DaysRemaining()
            };
        }
    }
}