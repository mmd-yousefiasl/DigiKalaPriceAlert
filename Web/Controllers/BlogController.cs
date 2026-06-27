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
        private readonly IBlogViewService _viewService;
        #endregion

        #region Ctor
        public BlogController(IBlogService blogService,
            IBlogViewService viewService)
        {
            _blogService = blogService;
            _viewService = viewService;
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

            filter.Category = category;
            filter.TakeEntity = 9;

            var result = await _blogService.ClientSideFilterBlogs(filter);
            if (result.IsFailure) return View(new ClientSideFilterBlogsDto());

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
            await _viewService.ClientSideAddBlogView(result.Value.Id);

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
            await _viewService.ClientSideAddBlogView(result.Value.Id);

            return View(result.Value);
        }
    }
}
