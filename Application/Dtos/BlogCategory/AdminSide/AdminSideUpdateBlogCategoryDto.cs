using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.BlogCategory.AdminSide
{
    public class AdminSideUpdateBlogCategoryDto : CommonDto
    {
        [Display(Name = "عنوان")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(100, ErrorMessage = Messages.MaxLengthError)]
        public string Title { get; set; }

        [Display(Name = "عنوان در مرورگر")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(200, ErrorMessage = Messages.MaxLengthError)]
        public string Slug { get; set; }
    }
}
