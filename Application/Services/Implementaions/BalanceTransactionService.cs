using Application.Dtos.BalanceTransaction.AdminSide;
using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.Users;
using Domain.Models.Users;
using Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Implementaions
{
    public class BalanceTransactionService : IBalanceTransactionService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor
        public BalanceTransactionService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public async Task<Result> AdminSideCreateNewTransaction(AdminSideCreateNewBalanceTransactionDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var user = await _context.Set<User>().FindAsync(model.UserId);
            if (user is null) return Result.Failure(Messages.OperationFailedError);

            // Create the transaction
            var transaction = new BalanceTransaction()
            {
                Amount = model.Amount,
                Type = model.Type,
                Description = model.Description,
                UserId = model.UserId,
                IsSuccess = true,
            };

            // Update user's balance
            if (model.Type == BalanceTransactionTypeEnum.Credit) user.Balance += model.Amount;
            else user.Balance -= model.Amount;

            if (user.Balance < 0) return Result.Failure(Messages.UserBalnaceCantBeLessThanZeroError);

            // Save both transaction and updated user
            _context.BalanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<AdminSideFilterBalanceTransactionsDto>> AdminSideFilterTransactions(AdminSideFilterBalanceTransactionsDto filter)
        {
            if (filter is null)
                return Result.Failure<AdminSideFilterBalanceTransactionsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<BalanceTransaction>();

            conditions.Add(n => n.UserId == filter.UserId);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.TransId.Contains(filter.Search) ||
                n.Description.Contains(filter.Search));
            }

            if (filter.Type is not null)
            {
                conditions.Add(n => n.Type == filter.Type);
            }

            if (filter.IsSuccess is not null)
            {
                conditions.Add(n => n.IsSuccess == filter.IsSuccess);
            }

            IQueryable<BalanceTransaction> query = _context.BalanceTransactions;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<BalanceTransaction>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideBalanceTransactionDto()
            {
                Id = n.Id,
                Amount = n.Amount,
                Description = n.Description,
                IsSuccess = n.IsSuccess,
                TransId = n.TransId,
                Type = n.Type,
                CreatedDate = n.CreatedDate,
            }));

            return filter;
        }

        public async Task<Result<ClientSideFilterBalanceTransactionsDto>> ClientSideFilterTransactions(ClientSideFilterBalanceTransactionsDto filter)
        {

            if (filter is null)
                return Result.Failure<ClientSideFilterBalanceTransactionsDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideFilterBalanceTransactionsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<BalanceTransaction>();

            conditions.Add(n => n.UserId == userId);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.TransId.Contains(filter.Search) ||
                n.Description.Contains(filter.Search));
            }

            if (filter.Type is not null)
            {
                conditions.Add(n => n.Type == filter.Type);
            }

            if (filter.IsSuccess is not null)
            {
                conditions.Add(n => n.IsSuccess == filter.IsSuccess);
            }

            IQueryable<BalanceTransaction> query = _context.BalanceTransactions;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<BalanceTransaction>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new ClientSideBalanceTransactionDto()
            {
                Amount = n.Amount,
                Description = n.Description,
                IsSuccess = n.IsSuccess,
                TransId = n.TransId,
                Type = n.Type,
                CreatedDate = n.CreatedDate,
            }));

            return filter;
        }
    }
}