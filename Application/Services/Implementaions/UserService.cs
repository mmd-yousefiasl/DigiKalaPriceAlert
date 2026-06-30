using Application.Dtos.BalanceTransaction.AdminSide;
using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Dtos.Products.ClientSide;
using Application.Dtos.Role.AdminSide;
using Application.Dtos.SendSms.ClientSide;
using Application.Dtos.Users.AdminSide;
using Application.Dtos.Users.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.Users;
using Domain.Models.Users;
using Domain.Shared;
using Domain.Statics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class UserService : IUserService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISendSmsService _sendSmsService;
        #endregion

        #region Ctor
        public UserService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ISendSmsService sendSmsService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _sendSmsService = sendSmsService;
        }
        #endregion

        public async Task<bool> IsSuperAdmin(int userId)
        {
            if (await _context.Users.AnyAsync(user => user.Id == userId && user.UserRoleMapping.Any(mapping => mapping.RoleId == 1)))
                return true;

            return false;
        }

        public async Task<bool> CheckUserRoleIds(int userId, List<int> roleIds)
        {
            if (await _context.Users.AnyAsync(user => user.Id == userId && user.UserRoleMapping.Any(mapping => roleIds.Contains(mapping.RoleId))))
                return true;

            return false;
        }

        public async Task<Result> SendUserLoginCode(ClientSideLoginDto model)
        {
            if (model is null)
                return Result.Failure(Messages.OperationFailedError);

            User? loginUser = await _context.Users.Where(n => n.Mobile == model.Mobile).FirstOrDefaultAsync();
            if (loginUser is null)
            {
                loginUser = new User();

                loginUser.Mobile = model.Mobile;
                loginUser.IsUserBanned = false;

                _context.Users.Add(loginUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (loginUser.IsUserBanned)
                    return Result.Failure(Messages.OperationFailedError);
            }

            var validationResult = await GenerateUserValidationCode(model.Mobile);
            if (validationResult.IsFailure)
                return Result.Failure(validationResult.Message!);

            var smsResult = await _sendSmsService.ClientSideSendLoginCode(new ClientSideSendLoginCodeDto()
            {
                Code = validationResult.Value,
                Mobile = loginUser.Mobile
            });

            return Result.Success();
        }

        public async Task<Result<ClientSideUserDto>> VerifyLoginCode(ClientSideLoginCodeDto model)
        {
            if (model is null) return Result.Failure<ClientSideUserDto>(Messages.OperationFailedError);

            string loginCode = string.Join("", model.LoginCode);
            var validationResult = await ValidateUserLoginCode(model.Mobile, loginCode);
            if (validationResult.IsFailure) return Result.Failure<ClientSideUserDto>(validationResult.Message);

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
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsUserBanned = u.IsUserBanned,
                    Mobile = u.Mobile,
                    SubscriptionEndDate = u.SubscriptionEndDate,
                    Balance = u.Balance,
                    UserRoles = u.UserRoleMapping.Where(urm => urm.UserId == u.Id)
                    .Select(urm => new AdminSideRoleDto()
                    {
                        Id = urm.RoleId,
                        Title = urm.Role.Title,
                        UniqueName = urm.Role.UniqueName
                    }).ToList()
                }).FirstOrDefaultAsync();
            if (user is null)
                return Result.Failure<AdminSideUserDetailsDto>(Messages.OperationFailedError);

            return user;
        }

        public async Task<Result<ClientSideUserDetailsDto>> ClientSideGetUserDetails()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideUserDetailsDto>(Messages.OperationFailedError);

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new ClientSideUserDetailsDto()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsUserBanned = u.IsUserBanned,
                    Mobile = u.Mobile,
                    SubscriptionEndDate = u.SubscriptionEndDate,
                    Balance = u.Balance,
                    UserRoles = u.UserRoleMapping.Where(urm => urm.UserId == u.Id)
                    .Select(urm => new AdminSideRoleDto()
                    {
                        Id = urm.RoleId,
                        Title = urm.Role.Title,
                        UniqueName = urm.Role.UniqueName
                    }).ToList()
                }).FirstOrDefaultAsync();
            if (user is null)
                return Result.Failure<ClientSideUserDetailsDto>(Messages.OperationFailedError);

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
                Description = $"کاهش موجودی کیف پول جهت تسویه حساب سفارش #{dto.OrderId}"
            });

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> ClientSideAddToUserSubscription(int durationInDays)
        {
            if (durationInDays < 1)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                return Result.Failure(Messages.OperationFailedError);

            if (user.SubscriptionEndDate == null || user.SubscriptionEndDate <= DateTime.UtcNow)
            {
                user.SubscriptionEndDate = DateTime.UtcNow.AddDays(durationInDays);
            }
            else
            {
                user.SubscriptionEndDate = user.SubscriptionEndDate.Value.AddDays(durationInDays);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> ClientSideAddRefundTransaction(decimal amount)
        {
            if (amount < 0) return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1) return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Users.Where(n => n.Id == userId).Include(n => n.Transactions).FirstOrDefaultAsync();
            if (user is null) return Result.Failure(Messages.OperationFailedError);

            user.Balance += amount;
            user.Transactions.Add(new BalanceTransaction()
            {
                IsSuccess = true,
                Amount = amount,
                TransId = null,
                Type = BalanceTransactionTypeEnum.Credit,
                Description = Messages.RefundUserBalanceMessage
            });

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> ClientSideCanUserAddProduct()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            if (await _context.UserProductMapping.CountAsync(n => n.UserId == userId) >= SiteTools.MaximumProductCount)
                return Result.Failure(Messages.UserReachedMaximumProductsError);

            return Result.Success();
        }

        public async Task<Result> ClientSideIsUserHaveSubscription(int userId)
        {
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var currentDate = DateTime.UtcNow;

            if (await _context.Users.AnyAsync(n => n.Id == userId && n.SubscriptionEndDate > currentDate))
                return Result.Success();

            return Result.Failure(Messages.OperationFailedError);
        }

        public async Task<Result<List<ClientSideEligibleProductIdWithUsersDto>>> ClientSideGetAllEligibleProductIdsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var result = await _context.UserProductMapping
                .AsNoTracking()
                .Where(upm => upm.User.SubscriptionEndDate > now)
                .GroupBy(upm => upm.ProductId)
                .Select(group => new ClientSideEligibleProductIdWithUsersDto
                {
                    Id = group.Key,
                    Users = group.Select(upm => new ClientSideUserDto
                    {
                        Id = upm.User.Id,
                        Mobile = upm.User.Mobile,
                        IsBanned = upm.User.IsUserBanned
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            if (result.Count == 0)
                return Result.Failure<List<ClientSideEligibleProductIdWithUsersDto>>(Messages.OperationFailedError);

            return Result.Success(result);
        }

        public async Task<List<string>> ClientSideGetUsersTrackingProduct(ClientSideGetUsersTrackingProductDto dto)
        {
            var now = DateTime.UtcNow;

            return await _context.UserProductMapping
                .AsNoTracking()
                .Where(up => up.ProductId == dto.ProductId && up.User.SubscriptionEndDate > now)
                .Select(up => up.User.Mobile)
                .ToListAsync(dto.CancellationToken);
        }

        public async Task<Result<string>> GenerateUserValidationCode(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return Result.Failure<string>(Messages.OperationFailedError);

            var existingValidations = await _context.UserValidation
                .Where(uv => uv.Mobile == mobile)
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();

            var latestValidation = existingValidations.FirstOrDefault();
            if (latestValidation != null &&
                DateTime.UtcNow - latestValidation.CreatedDate < TimeSpan.FromMinutes(2))
            {
                return Result.Failure<string>(Messages.ToManyLoginAttemptError);
            }

            if (existingValidations.Any())
                _context.UserValidation.RemoveRange(existingValidations);

            var code = Generate.UserLoginCode();
            var hashedCode = code.HashPassword(out string salt);

            var validation = new UserValidation
            {
                Code = hashedCode,
                Salt = salt,
                Mobile = mobile,
                CreatedDate = DateTime.UtcNow,
            };

            _context.UserValidation.Add(validation);
            await _context.SaveChangesAsync();

            return code;
        }

        public async Task<Result> ValidateUserLoginCode(string mobile, string code)
        {
            if (string.IsNullOrWhiteSpace(mobile) || string.IsNullOrWhiteSpace(code)) return Result.Failure(Messages.OperationFailedError);

            var validation = await _context.UserValidation.Where(uv => uv.Mobile == mobile)
                .FirstOrDefaultAsync();
            if (validation is null)
                return Result.Failure(Messages.OperationFailedError);

            if (!code.VerifyPassword(validation.Salt, validation.Code)) return Result.Failure(Messages.InvalidLoginCodeError);

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
    }
}