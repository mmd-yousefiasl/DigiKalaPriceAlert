using Application.Dtos.Users.BotSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result> BotSideCreateNewUser(BotSideCreateNewUserDto dto);

        Task<Result> BotSideCanUserAddProduct(long chatId);

        Task<Result> BotSideCreateNewUserProduct(BotSideCreateNewUserProductDto dto);

        Task<List<long>> BotSideGetUserChatIdsTrackingThisProduct(BotSideGetUsersTrackingProductDto dto);

        Task<Result> BotSideDoesUserExist(long chatId);

        Task<Result<int>> GetUserIdByChatId(long chatId);
    }
}
