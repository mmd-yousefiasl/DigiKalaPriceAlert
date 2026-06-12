using Application.Services.Interfaces;
using Data.Context;
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
    }
}
