using Application.Dtos.Role.AdminSide;
using Application.Dtos.Users.AdminSide;
using Application.Dtos.Users.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Users;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class UserService : IUserService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IUserValidationService _userValidationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor
        public UserService(DigiKalaPriceAlertDbContext context,
            IUserValidationService userValidationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userValidationService = userValidationService;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<Result<string>> SendUserLoginCode(ClientSideLoginDto model)
        {
            if (model is null) return Result.Failure<string>(Messages.OperationFailedError);

            User loginUser = await _context.Users.Where(n => n.Mobile == model.Mobile).FirstOrDefaultAsync();
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
                if (loginUser.IsUserBanned) return Result.Failure<string>(Messages.OperationFailedError);
            }

            var validationResult = await _userValidationService.GenerateUserValidationCode(model.Mobile);
            if (validationResult.IsFailure) return Result.Failure<string>(validationResult.Message);

            Console.WriteLine("Login Code: " + validationResult.Value);

            //await _smsServiec.SendLoginCode(validationResult.Value, loginUser.Mobile);

            return validationResult.Value;
        }

        public async Task<Result<ClientSideUserDto>> VerifyLoginCode(ClientSideLoginCodeDto model)
        {
            if (model is null) return Result.Failure<ClientSideUserDto>(Messages.OperationFailedError);

            string loginCode = string.Join("", model.LoginCode);
            var validationResult = await _userValidationService.ValidateUserLoginCode(model.Mobile, loginCode);
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
                n.LastName.Contains(filter.Search) || n.Username.Contains(filter.Search));
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
                Username = n.Username,
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
                    ChatId = u.ChatId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsUserBanned = u.IsUserBanned,
                    Mobile = u.Mobile,
                    Username = u.Username,
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
                    ChatId = u.ChatId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsUserBanned = u.IsUserBanned,
                    Mobile = u.Mobile,
                    Username = u.Username,
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
    }
}