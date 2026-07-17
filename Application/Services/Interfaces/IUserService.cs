using Application.Dtos.BalanceTransaction.AdminSide;
using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Dtos.Users.AdminSide;
using Application.Dtos.Users.ClientSide;
using Application.Dtos.UserValidation;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<Result> IsSuperAdmin(int userId);

        Task<Result> CheckUserRoleIds(int userId, List<int> roleIds);

        Task<Result<DateTime>> SendUserLoginCode(ClientSideLoginDto model);

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

        Task<Result> ClientSideCanUserAddProduct();

        Task<Result> ClientSideIsUserHaveSubscription(int userId);

        Task<Result<OtpGenerationResultDto>> GenerateUserValidationCode(string mobile);

        Task<Result> ValidateUserLoginCode(string mobile, string code);

        Task<Result<AdminSideFilterBalanceTransactionsDto>> AdminSideFilterTransactions(AdminSideFilterBalanceTransactionsDto filter);

        Task<Result> AdminSideCreateNewTransaction(AdminSideCreateNewBalanceTransactionDto model);

        Task<Result<ClientSideFilterBalanceTransactionsDto>> ClientSideFilterTransactions(ClientSideFilterBalanceTransactionsDto filter);

        Task<Result> ClientSideAddUnVerifiedTransaction(ClientSideCreateNewTransactionDto model);

        Task<Result> ClientSideVerifyTransaction(string transId);

        Task<Result> ClientSideIsUserProfileCompleted(int userId);

        Task<Result> ClientSideUpdateUserProfile(ClientSideUpdateProfileDto dto);

        Task<List<ClientSideUserToNotifyDto>> GetUserContactsAsync(List<int> userIds, CancellationToken ct);
    }
}
