using Application.Dtos.Blog.ClientSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class BlogController : BaseController
    {
        #region Fields
        private readonly IBlogService _blogService;
        #endregion

        #region Ctor
        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }
        #endregion

        [HttpGet("/blogs/{category?}")]
        public async Task<IActionResult> BlogsList(string? category, ClientSideFilterBlogsDto filter)
        {
            var query = HttpContext.Request.Query;

            if (query.Count == 0 && !string.IsNullOrWhiteSpace(category))
            {
                ViewData["NoIndex"] = false;
            }
            else if (query.Count > 0)
            {
                ViewData["NoIndex"] = true;
            }

            filter.CategorySlug = category;
            filter.TakeEntity = 9;

            var result = await _blogService.ClientSideFilterBlogs(filter);
            if (result.IsFailure) return View(new ClientSideFilterBlogsDto());

            if (string.IsNullOrWhiteSpace(category))
            {
                ViewData["MetaDescription"] = "مجله تخصصی فروشندگان دیجی‌کالا؛ جدیدترین استراتژی‌های مدیریت بای‌باکس، پایش قیمت رقبا و راهکارهای عملی برای افزایش سود و سهم بازار در مارکت‌پلیس.";
            }
            else
            {
                ViewData["MetaDescription"] = result.Value.CategoryMetaDescription;
            }

            return View(result.Value);
        }

        [HttpGet("/b/{id}/{slug}")]
        public async Task<IActionResult> BlogDetails(int id, string slug)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(BlogsList));
            }
            var result = await _blogService.ClientSideGetBlogDetails(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(BlogsList));
            }
            await _blogService.ClientSideAddBlogView(result.Value.Id);

            ViewData["MetaAuthor"] = result.Value.Writer;
            ViewData["MetaDescription"] = result.Value.MetaDescription;

            return View(result.Value);
        }

        [HttpGet("/b/{id}")]
        public async Task<IActionResult> BlogDetails(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(BlogsList));
            }
            var result = await _blogService.ClientSideGetBlogDetails(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(BlogsList));
            }
            await _blogService.ClientSideAddBlogView(result.Value.Id);

            ViewData["MetaAuthor"] = result.Value.Writer;
            ViewData["MetaDescription"] = result.Value.MetaDescription;

            return View(result.Value);
        }
    }
}
