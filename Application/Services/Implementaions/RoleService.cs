using Application.Dtos.Role.AdminSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Identity;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class RoleService : IRoleService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public RoleService(DigiKalaPriceAlertDbContext context,
            IUserService userService)
        {
            _context = context;
            _userService = userService;
        }
        #endregion

        public async Task<Result<bool>> UserCanAccessThisPermissions(int userId, string permission)
        {
            if (userId < 1 || permission is null) return Result.Failure<bool>(Messages.OperationFailedError);

            if (await _userService.IsSuperAdmin(userId))
                return true;

            var roleIds = await _context.Roles.Where(role => role.RolePermissionMapping.Any(mapping => mapping.Permission.UniqueName == permission)).Select(n => n.Id).ToListAsync();
            if (!roleIds?.Any() ?? true) return Result.Failure<bool>(Messages.OperationFailedError);

            return await _userService.CheckUserRoleIds(userId, roleIds);
        }

        public async Task<bool> CheckUserAccess(int userId, string permission)
        {
            if (userId < 1 || permission is null) return false;

            if (await _userService.IsSuperAdmin(userId))
                return true;

            var roleIds = await _context.Roles.Where(role => role.RolePermissionMapping.Any(mapping => mapping.Permission.UniqueName == permission)).Select(n => n.Id).ToListAsync();
            if (!roleIds?.Any() ?? true) return false;

            return await _userService.CheckUserRoleIds(userId, roleIds);
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

            return Result.Success(Messages.SuccessfullyDone);
        }
    }
}
