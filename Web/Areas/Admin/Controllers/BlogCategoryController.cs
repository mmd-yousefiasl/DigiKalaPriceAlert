using Application.Dtos.BlogCategory.AdminSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Web.Attributes;

namespace Web.Areas.Admin.Controllers
{
    [Permission("ManageBlogCategories")]
    public class BlogCategoryController : BaseAdminController
    {
        #region Fields
        private readonly IBlogService _blogService;
        #endregion

        #region Ctor
        public BlogCategoryController(IBlogService blogService)
        {
            _blogService = blogService;
        }
        #endregion

        [Permission("BlogCategoriesList")]
        [HttpGet("/admin/blog-categories-list")]
        public async Task<IActionResult> BlogCategoriesList(AdminSideFilterBlogCategoriesDto filter)
        {
            var result = await _blogService.AdminSideFilterCategories(filter);
            if (result.IsFailure) return View(new AdminSideFilterBlogCategoriesDto());

            return View(result.Value);
        }

        [Permission("CreateNewBlogCategory")]
        [HttpGet("/admin/add-new-blog-category")]
        public IActionResult CreateNewBlogCategory()
        {
            return View();
        }

        [Permission("CreateNewBlogCategory")]
        [HttpPost("/admin/add-new-blog-category"), ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewBlogCategory(AdminSideCreateBlogCategoryDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _blogService.AdminSideCreateNewBlogCategory(model);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(model);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(BlogCategoriesList));
        }

        [Permission("UpdateBlogCategory")]
        [HttpGet("/admin/edit-blog-category")]
        public async Task<IActionResult> UpdateBlogCategory(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(BlogCategoriesList));
            }
            var result = await _blogService.AdminSideGetCategoryForUpdate(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(BlogCategoriesList));
            }

            return View(result.Value);
        }

        [Permission("UpdateBlogCategory")]
        [HttpPost("/admin/edit-blog-category"), ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBlogCategory(AdminSideUpdateBlogCategoryDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _blogService.AdminSideUpdateCategory(model);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(model);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(BlogCategoriesList));
        }

        [Permission("DeleteBlogCategory")]
        [HttpPost("/admin/remove-blog-category"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlogCategory(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(BlogCategoriesList));
            }
            var result = await _blogService.AdminSideDeleteCategory(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(BlogCategoriesList));
        }
    }
}