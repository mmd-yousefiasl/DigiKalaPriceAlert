using Application.Dtos.SubscriptionPlans.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ISubscriptionPlanService
    {
        Task<Result<ClientSideSubscriptionPlanDto>> ClientSideGetPlanById(int id);
    }
}
