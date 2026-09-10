using Domain.Enums.SubscriptionPlans;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IUsedFreePlanService
    {
        Task<Result> ClientSideCanUserGetFreePlan(UsedFreePlanTypeEnum type);

        Task<Result> ClientSideCreateNewUsedFreePlan(UsedFreePlanTypeEnum typeEnum);
    }
}
