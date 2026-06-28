using Application.Dtos.BlogCategory.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Components
{
    public class BlogCategoriesViewComponent : ViewComponent
    {
        private readonly IBlogService _blogService;
        public BlogCategoriesViewComponent(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _blogService.ClientSideGetCategories();
            if (result.IsFailure) return View("BlogCategories", new List<ClientSideBlogCategoryDto>());

            return View("BlogCategories", result.Value);
        }
    }
}
