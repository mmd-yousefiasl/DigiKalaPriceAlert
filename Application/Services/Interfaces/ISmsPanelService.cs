using Application.Dtos.SmsPanel.AdminSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ISmsPanelService
    {
        Task<Result> AdminSideCreateNewSmsPanel(AdminSideCreateNewSmsPanelDto dto);

        Task<Result<AdminSideUpdateSmsPanelDto>> AdminSideGetSmsPanelForUpdate(int id);

        Task<Result> AdminSideUpdateSmsPanel(AdminSideUpdateSmsPanelDto dto);

        Task<Result<AdminSideFilterSmsPanelsDto>> AdminSideFilterSmsPanles(AdminSideFilterSmsPanelsDto filter);

        Task<Result> AdminSideDeleteSmsPanel(int id);
    }
}
