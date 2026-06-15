using Application.Dtos.Blog.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Components
{
    public class RelatedBlogsViewComponent : ViewComponent
    {
        private readonly IBlogService _blogService;
        public RelatedBlogsViewComponent(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int blogId, int categoryId)
        {
            var result = await _blogService.ClientSideGetRelatedBlogs(blogId, categoryId);
            if (result.IsFailure) return View("RelatedBlogs", new List<ClientSideBlogDto>());

            return View("RelatedBlogs", result.Value);
        }
    }
}
