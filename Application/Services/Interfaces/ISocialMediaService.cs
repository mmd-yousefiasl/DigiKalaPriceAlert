using Application.Dtos.Media.AdminSide;
using Application.Dtos.Media.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ISocialMediaService
    {
        Task<Result<AdminSideFilterSocialMediaDto>> AdminSideFilterSocialMedia(AdminSideFilterSocialMediaDto filter);

        Task<Result> AdminSideCreateNewSocialMedia(AdminSideCreateNewSocialMediaDto model);

        Task<Result<AdminSideUpdateSocialMediaDto>> AdminSideGetSocialMediaForUpdate(int id);

        Task<Result> AdminSideUpdateSocialMedia(AdminSideUpdateSocialMediaDto model);

        Task<Result> AdminSideDeleteSocialMedia(int id);

        Task<Result<List<ClientSideSocialMediaDto>>> ClientSideGetSocialMedia();
    }
}
