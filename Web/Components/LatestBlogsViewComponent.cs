using Application.Dtos.Blog.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Components
{
    public class LatestBlogsViewComponent : ViewComponent
    {
        private readonly IBlogService _blogService;
        public LatestBlogsViewComponent(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _blogService.ClientSideGetHighPriorityBlogs(5);
            if (result.IsFailure) return View("LatestBlogs", new List<ClientSideBlogDto>());

            return View("LatestBlogs", result.Value);
        }
    }
}
