using Application.Dtos.Role.AdminSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IRoleService
    {
        Task<Result> UserCanAccessThisPermissions(int userId, string permission);

        Task<Result> CheckUserAccess(int userId, string permission);

        Task<Result<List<AdminSideRoleDto>>> AdminSideGetAllRolesForUserCRUD();

        Task<Result<AdminSideFilterRolesDto>> AdminSideFilterRoles(AdminSideFilterRolesDto filter);

        Task<Result> AdminSideCreateNewRole(AdminSideCreateNewRoleDto model);

        Task<Result> AdminSideDeleteRole(int id);

        Task<Result<AdminSideUpdateRoleDto>> AdminSideGetRoleForUpdate(int id);

        Task<Result> AdminSideUpdateRole(AdminSideUpdateRoleDto model);

        Task<Result<List<int>>> GetRoleIdsForPermission(string permission);
    }
}
