using Application.Dtos.Permission.AdminSide;
using Application.Services.Interfaces;
using Data.Context;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class PermissionService : IPermissionService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public PermissionService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result<List<AdminSidePermissionDto>>> AdminSideGetAllPermissions()
        {
            var permissions = await _context.Permissions
                .Select(p => new AdminSidePermissionDto()
                {
                    Id = p.Id,
                    ParentId = p.ParentId,
                    Title = p.Title
                })
                .AsNoTracking().ToListAsync();

            if (!permissions?.Any() ?? true)
                return Result.Failure<List<AdminSidePermissionDto>>(Messages.OperationFailedError);

            return permissions;
        }
    }
}
