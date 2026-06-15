using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.BlogCategory.AdminSide
{
    public class AdminSideFilterBlogCategoriesDto : BasePaging<AdminSideBlogCategoryDto>
    {
        [Display(Name = "جست و جو")]
        public string? Search { get; set; }
    }
}
