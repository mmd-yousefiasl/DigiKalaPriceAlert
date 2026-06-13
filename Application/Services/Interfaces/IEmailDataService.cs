using Application.Dtos.EmailData.AdminSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IEmailDataService
    {
        Task<Result<AdminSideEmailDataDto>> AdminSideGetEmailData();

        Task<Result<AdminSideUpdateEmailDataDto>> AdminSideGetEmailDataForUpdate();

        Task<Result> AdminSideUpdateEmailData(AdminSideUpdateEmailDataDto model);
    }
}
