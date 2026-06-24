using Application.Dtos.Users.AdminSide;
using Application.Dtos.Users.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> IsSuperAdmin(int userId);

        Task<bool> CheckUserRoleIds(int userId, List<int> roleIds);

        Task<Result<string>> SendUserLoginCode(ClientSideLoginDto model);

        Task<Result<ClientSideUserDto>> VerifyLoginCode(ClientSideLoginCodeDto model);

        Task<Result<AdminSideFilterUsersDto>> AdminSideFilterUsers(AdminSideFilterUsersDto filter);

        Task<Result> AdminSideCreateNewUser(AdminSideCreateNewUserDto model);

        Task<Result> AdminSideDeleteUser(int id);

        Task<Result<AdminSideUpdateUserDto>> AdminSideGetUserForUpdate(int id);

        Task<Result> AdminSideUpdateUser(AdminSideUpdateUserDto model);

        Task<Result<AdminSideUserDetailsDto>> AdminSideGetUserDetails(int id);

        Task<Result<ClientSideUserDetailsDto>> ClientSideGetUserDetails();

        Task<decimal> ClientSideGetCurrentUserBalance();

        Task<Result> ClientSideWithdrawUserOrder(ClientSideWithdrawUserOrderDto dto);

        Task<Result> ClientSideAddToUserSubscription(int durationInDays);

        Task<Result> ClientSideAddRefundTransaction(decimal amount);

        Task<Result> ClientSideCanUserAddProduct();

        Task<Result> ClientSideIsUserHaveSubscription(int userId);
    }
}
