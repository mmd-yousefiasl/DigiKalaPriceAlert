using Domain.Shared;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Media.AdminSide
{
    public class AdminSideUpdateSocialMediaDto : CommonDto
    {
        [Display(Name = "تصویر")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(50, ErrorMessage = Messages.MaxLengthError)]
        public string FileName { get; set; }

        [Display(Name = "تصویر")]
        public IFormFile? File { get; set; }

        [Display(Name = "عنوان")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(100, ErrorMessage = Messages.MaxLengthError)]
        public string Title { get; set; }

        [Display(Name = "لینک")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(500, ErrorMessage = Messages.MaxLengthError)]
        public string Url { get; set; }

        [Display(Name = "اولویت")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [Range(1, 100, ErrorMessage = Messages.RangeError)]
        public int Priority { get; set; }

        [Display(Name = "وضعیت انتشار")]
        [Required(ErrorMessage = Messages.RequiredError)]
        public bool IsPublished { get; set; }
    }
}
