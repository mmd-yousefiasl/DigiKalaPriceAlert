using Application.Dtos.SubscriptionPlans.ClientSide;
using Application.Services.Interfaces;
using Data.Context;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public SubscriptionPlanService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result<ClientSideSubscriptionPlanDto>> ClientSideGetPlanById(int id)
        {
            if (id < 1)
                return Result.Failure<ClientSideSubscriptionPlanDto>(Messages.InvalidPlanError);

            var plan = await _context.SubscriptionPlans.Where(n => n.Id == id)
                .Select(n => new ClientSideSubscriptionPlanDto()
                {
                    Id = n.Id,
                    Name = n.Name,
                    Price = n.Price
                }).AsNoTracking()
                .FirstOrDefaultAsync();
            if (plan is null)
                return Result.Failure<ClientSideSubscriptionPlanDto>(Messages.InvalidPlanError);

            return plan;
        }
    }
}
