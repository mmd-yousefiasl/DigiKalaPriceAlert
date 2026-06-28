using Application.Dtos.Blog.AdminSide;
using Application.Dtos.Blog.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IBlogService
    {
        Task<Result<AdminSideFIlterBlogsDto>> AdminSideFilterBlogs(AdminSideFIlterBlogsDto filter);

        Task<Result> AdminSideCreateNewBlog(AdminSideCreateNewBlogDto model);

        Task<Result<AdminSideUpdateBlogDto>> AdminSideGetBlogForUpdate(int id);

        Task<Result> AdminSideUpdateBlog(AdminSideUpdateBlogDto model);

        Task<Result> AdminSideDeleteBlog(int id);

        Task<Result<List<ClientSideBlogDto>>> ClientSideGetHighPriorityBlogs(int take);

        Task<Result<ClientSideFilterBlogsDto>> ClientSideFilterBlogs(ClientSideFilterBlogsDto filter);

        Task<Result<ClientSideBlogDetailsDto>> ClientSideGetBlogDetails(int id);

        Task<Result<List<ClientSideBlogDto>>> ClientSideGetRelatedBlogs(int blogId, int categoryId);

        Task<Result<List<AdminSideBlogForSiteMapDto>>> AdminSideGetBlogIdsForSiteMap();
    }
}
