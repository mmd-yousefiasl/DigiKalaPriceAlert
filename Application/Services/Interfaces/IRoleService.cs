using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IRoleService
    {
        Task<Result<bool>> UserCanAccessThisPermissions(int userId, string permission);

        Task<bool> CheckUserAccess(int userId, string permission);
    }
}
