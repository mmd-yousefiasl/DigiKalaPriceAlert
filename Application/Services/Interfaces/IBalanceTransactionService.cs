using Application.Dtos.BalanceTransaction.AdminSide;
using Application.Dtos.BalanceTransaction.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IBalanceTransactionService
    {
        Task<Result<AdminSideFilterBalanceTransactionsDto>> AdminSideFilterTransactions(AdminSideFilterBalanceTransactionsDto filter);

        Task<Result> AdminSideCreateNewTransaction(AdminSideCreateNewBalanceTransactionDto model);

        Task<Result<ClientSideFilterBalanceTransactionsDto>> ClientSideFilterTransactions(ClientSideFilterBalanceTransactionsDto filter);
    }
}
