using Application.Dtos.SiteMap.AdminSide;
using Application.Dtos.SiteMap.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ISiteMapService
    {
        Task<Result<AdminSideFilterSiteMapDto>> AdminSideFilterSiteMap(AdminSideFilterSiteMapDto filter);

        Task<Result> AdminSideCreateNewSiteMap(AdminSideCreateNewSiteMapDto model);

        Task<Result<AdminSideUpdateSiteMapDto>> AdminSideGetSiteMapForUpdate(int id);

        Task<Result> AdminSideUpdateSiteMap(AdminSideUpdateSiteMapDto model);

        Task<Result> AdminSideDeleteSiteMap(int id);

        Task<Result> AdminSideDeleteAllLinks();

        Task<Result> AdminSideDeleteAllExceptOtherPages();

        Task<Result> UpdateBlogCategoriesSiteMap();

        Task<Result> UpdateBlogsSiteMap();

        Task<Result> UpdateAllSiteMap();

        Task<Result<List<ClientSideSiteMapDto>>> ClientSideGetSiteMap();
    }
}
