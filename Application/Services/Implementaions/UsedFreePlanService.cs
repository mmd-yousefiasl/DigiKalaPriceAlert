using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.SubscriptionPlans;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class UsedFreePlanService : IUsedFreePlanService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor
        public UsedFreePlanService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public async Task<Result> ClientSideCanUserGetFreePlan(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return Result.Failure(Messages.OperationFailedError);

            if (await _context.UsedFreePlans.AnyAsync(n => n.Mobile == mobile))
                return Result.Failure(Messages.YouAlreadyUsedFreePlanError);

            return Result.Success(Messages.FreePlanActivatedSuccessfully);
        }

        public async Task<Result> ClientSideCreateNewUsedFreePlan(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            if (await _context.UsedFreePlans.AnyAsync(n => n.Mobile == mobile))
                return Result.Failure(Messages.OperationFailedError);

            var usedFreePlan = new UsedFreePlan()
            {
                Mobile = mobile,
                UserId = userId,
            };

            _context.UsedFreePlans.Add(usedFreePlan);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
