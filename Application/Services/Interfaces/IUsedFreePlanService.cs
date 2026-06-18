using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IUsedFreePlanService
    {
        Task<Result> ClientSideCanUserGetFreePlan(string mobile);

        Task<Result> ClientSideCreateNewUsedFreePlan(string mobile);
    }
}
