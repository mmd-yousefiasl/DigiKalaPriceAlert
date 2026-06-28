using Application.Dtos.BlogCategory.AdminSide;
using Application.Dtos.BlogCategory.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IBlogCategoryService
    {
        Task<Result<AdminSideFilterBlogCategoriesDto>> AdminSideFilterCategories(AdminSideFilterBlogCategoriesDto filter);

        Task<Result> AdminSideCreateNewBlogCategory(AdminSideCreateBlogCategoryDto model);

        Task<Result<AdminSideUpdateBlogCategoryDto>> AdminSideGetCategoryForUpdate(int id);

        Task<Result> AdminSideUpdateCategory(AdminSideUpdateBlogCategoryDto model);

        Task<Result> AdminSideDeleteCategory(int id);

        Task<Result<List<ClientSideBlogCategoryDto>>> ClientSideGetCategories();

        Task<Result<List<AdminSideBlogCategoryAsOptionDto>>> AdminSideGetCategoriesAsOptions();
    }
}
