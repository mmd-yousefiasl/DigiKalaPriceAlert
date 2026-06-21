using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IUsedFreePlanService
    {
        Task<Result> ClientSideCanUserGetFreePlan();

        Task<Result> ClientSideCreateNewUsedFreePlan();
    }
}
