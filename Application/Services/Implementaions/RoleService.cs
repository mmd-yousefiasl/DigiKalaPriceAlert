using Application.Dtos.Permission;
using Application.Dtos.Role.AdminSide;
using Application.Mediator;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Events.Roles;
using Domain.Models.Identity;
using Domain.Shared;
using Domain.Statics;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class RoleService : IRoleService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IUserService _userService;
        private readonly IMediatorHandler _mediator;
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public RoleService(DigiKalaPriceAlertDbContext context,
            IUserService userService,
            IMediatorHandler mediator,
            ICacheManager cacheManager)
        {
            _context = context;
            _userService = userService;
            _cacheManager = cacheManager;
            _mediator = mediator;
        }
        #endregion

        public async Task<Result> UserCanAccessThisPermissions(int userId, string permission)
        {
            if (userId < 1 || string.IsNullOrWhiteSpace(permission))
                return Result.Failure(Messages.OperationFailedError);

            var superAdminResult = await _userService.IsSuperAdmin(userId);
            if (superAdminResult.IsSuccess)
                return Result.Success();

            var roleIdsResult = await GetRoleIdsForPermission(permission);
            if (roleIdsResult.IsFailure)
                return Result.Failure(Messages.OperationFailedError);

            return await _userService.CheckUserRoleIds(userId, roleIdsResult.Value);
        }

        public async Task<Result> CheckUserAccess(int userId, string permission)
        {
            if (userId < 1 || string.IsNullOrWhiteSpace(permission))
                return Result.Failure(Messages.OperationFailedError);

            var superAdminResult = await _userService.IsSuperAdmin(userId);
            if (superAdminResult.IsSuccess)
                return Result.Success();

            var roleIdsResult = await GetRoleIdsForPermission(permission);
            if (roleIdsResult.IsFailure)
                return Result.Failure(Messages.OperationFailedError);

            return await _userService.CheckUserRoleIds(userId, roleIdsResult.Value);
        }

        public async Task<Result<List<AdminSideRoleDto>>> AdminSideGetAllRolesForUserCRUD()
        {
            var roles = await _context.Roles
                .Select(n => new AdminSideRoleDto()
                {
                    Id = n.Id,
                    Title = n.Title,
                    UniqueName = n.UniqueName
                }).AsNoTracking().ToListAsync();

            if (!roles?.Any() ?? true) return Result.Failure<List<AdminSideRoleDto>>(Messages.OperationFailedError);

            return roles;
        }

        public async Task<Result<AdminSideFilterRolesDto>> AdminSideFilterRoles(AdminSideFilterRolesDto filter)
        {
            if (filter is null)
                return Result.Failure<AdminSideFilterRolesDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Role>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Title.Contains(filter.Search) || n.UniqueName.Contains(filter.Search));
            }

            IQueryable<Role> query = _context.Roles;

            query = query.OrderByDescending(n => n.Id);

            foreach (var condition in conditions ?? new FilterConditions<Role>())
            {
                query = query.Where(condition);
            }

            await filter.Paging(query.Select(n => new AdminSideRoleDto()
            {
                Id = n.Id,
                Title = n.Title,
                UniqueName = n.UniqueName
            }));

            return filter;
        }

        public async Task<Result> AdminSideCreateNewRole(AdminSideCreateNewRoleDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            if (await _context.Roles.AnyAsync(n => n.UniqueName == model.UniqueName))
                return Result.Failure(Messages.DuplicateUniqueNameForRoleError);

            Role role = new Role()
            {
                Title = model.Title,
                UniqueName = model.UniqueName,
                RolePermissionMapping = new List<RolePermissionMapping>()
            };

            foreach (var item in model.PermissionIds)
            {
                role.RolePermissionMapping.Add(new RolePermissionMapping()
                {
                    RoleId = role.Id,
                    PermissionId = item
                });
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            RoleCreatedEvent RoleCreatedEvent = new RoleCreatedEvent(
            role.Title,
            role.UniqueName);

            await _mediator.PublishEvent(RoleCreatedEvent);

            return Result.Success(Messages.SuccessfullyDone);
        }

        public async Task<Result> AdminSideDeleteRole(int id)
        {
            if (id < 1) return Result.Failure(Messages.OperationFailedError);

            var role = await _context.Roles.FindAsync(id);
            if (role is null) return Result.Failure(Messages.OperationFailedError);

            role.IsDeleted = true;
            role.LastUpdatedDate = DateTime.UtcNow;

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            RoleDeletedEvent RoleDeletedEvent = new RoleDeletedEvent(role.Id);

            await _mediator.PublishEvent(RoleDeletedEvent);

            return Result.Success();
        }

        public async Task<Result<AdminSideUpdateRoleDto>> AdminSideGetRoleForUpdate(int id)
        {
            if (id < 1)
                return Result.Failure<AdminSideUpdateRoleDto>(Messages.OperationFailedError);

            var role = await _context.Roles.Where(n => n.Id == id).Include(n => n.RolePermissionMapping).Select(n => new AdminSideUpdateRoleDto()
            {
                Id = n.Id,
                PermissionIds = n.RolePermissionMapping.Select(n => n.PermissionId).ToList(),
                Title = n.Title,
                UniqueName = n.UniqueName
            }).AsNoTracking()
            .FirstOrDefaultAsync();

            if (role is null) return Result.Failure<AdminSideUpdateRoleDto>(Messages.OperationFailedError);

            return role;
        }

        public async Task<Result> AdminSideUpdateRole(AdminSideUpdateRoleDto model)
        {
            var role = await _context.Roles.Include(n => n.RolePermissionMapping).FirstOrDefaultAsync(n => n.Id == model.Id);

            if (role is null) return Result.Failure(Messages.OperationFailedError);


            if (await _context.Roles.AnyAsync(n => n.UniqueName == model.UniqueName && n.Id != model.Id))
                return Result.Failure(Messages.DuplicateUniqueNameForRoleError);

            foreach (var permission in role.RolePermissionMapping)
            {
                role.RolePermissionMapping.Remove(permission);
            }

            if (role.UniqueName == "Admin")
                return Result.Failure(Messages.OperationFailedError);

            role.Title = model.Title;
            role.UniqueName = model.UniqueName;
            role.LastUpdatedDate = DateTime.UtcNow;

            foreach (var item in model.PermissionIds ?? new List<int>())
            {
                role.RolePermissionMapping.Add(new RolePermissionMapping
                {
                    RoleId = role.Id,
                    PermissionId = item
                });
            }

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();

            RoleUpdatedEvent RoleUpdatedEvent = new RoleUpdatedEvent(
            role.Id,
            role.Title,
            role.UniqueName);

            await _mediator.PublishEvent(RoleUpdatedEvent);

            return Result.Success(Messages.SuccessfullyDone);
        }

        public async Task<Result<List<int>>> GetRoleIdsForPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return Result.Failure<List<int>>(Messages.OperationFailedError);

            var permissionsRoleIds = await _cacheManager.GetAsync(CacheKeys.PermissionsRoleIdsCacheKey, async () =>
            {
                var rawData = await _context.RolePermissionMapping
                .AsNoTracking()
                .Select(mapping => new
                {
                    PermissionName = mapping.Permission.UniqueName,
                    RoleId = mapping.RoleId
                }).ToListAsync();

                return rawData
                .GroupBy(x => x.PermissionName)
                .Select(g => new PermissionRoleIdsDto
                {
                    UniqueName = g.Key,
                    RoleIds = g.Select(x => x.RoleId).ToList()
                }).ToList();
            });

            if (!permissionsRoleIds?.Any() ?? true)
                return Result.Failure<List<int>>(Messages.OperationFailedError);

            var matchedPermission = permissionsRoleIds!
                .FirstOrDefault(p => p.UniqueName == permission);
            if (matchedPermission is null || (!matchedPermission.RoleIds?.Any() ?? true))
                return Result.Failure<List<int>>(Messages.OperationFailedError);

            return matchedPermission.RoleIds;
        }
    }
}
