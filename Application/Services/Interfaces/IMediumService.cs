using Application.Dtos.Medium.AdminSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IMediumService
    {
        Task<Result<AdminSideFilterMediaDto>> AdminSideFilterMedia(AdminSideFilterMediaDto filter);

        Task<Result> AdminSideCreateNewMedia(AdminSideCreateNewMediumDto dto);

        Task<Result> AdminSideDeleteMedia(int id);
    }
}
