using Application.Dtos.Medium.AdminSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IMediumService
    {
        Task<Result<AdminSideFilterMediaDto>> AdminSideFilterMedia(AdminSideFilterMediaDto filter);

        Task<Result> AdminSideCreateNewMedium(AdminSideCreateNewMediumDto dto);

        Task<Result> AdminSideDeleteMedium(int id);
    }
}
