using Application.Dtos.BalanceTransaction.AdminSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IBalanceTransactionService
    {
        Task<Result<AdminSideFilterBalanceTransactionsDto>> AdminSideFilterTransactions(AdminSideFilterBalanceTransactionsDto filter);

        Task<Result> AdminSideCreateNewTransaction(AdminSideCreateNewBalanceTransactionDto model);
    }
}
