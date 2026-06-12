using Application.Dtos.SubscriptionPlans.BotSide;
using Application.Dtos.Users.BotSide;
using Application.Dtos.Users.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> IsSuperAdmin(int userId);

        Task<bool> CheckUserRoleIds(int userId, List<int> roleIds);

        Task<Result> BotSideCreateNewUser(BotSideCreateNewUserDto dto);

        Task<Result> BotSideCanUserAddProduct(long chatId);

        Task<Result> BotSideCreateNewUserProduct(BotSideCreateNewUserProductDto dto);

        Task<Result> BotSideDoesUserExist(long chatId);

        Task<Result<int>> GetUserIdByChatId(long chatId);

        Task<Result<BotSideSubscriptionPlanDto>> GetCurrenUserSubscriptionPlanByChatId(long chatId);

        Task<List<BotSideUserTrackingProductDto>> GetUsersTrackingProduct(BotSideGetUsersTrackingProductDto dto);

        Task<Result<List<int>>> GetAllEligibleProductIdsAsync(CancellationToken cancellationToken);

        Task<Result<string>> SendUserLoginCode(ClientSideLoginDto model);

        Task<Result<ClientSideUserDto>> VerifyLoginCode(ClientSideLoginCodeDto model);
    }
}
