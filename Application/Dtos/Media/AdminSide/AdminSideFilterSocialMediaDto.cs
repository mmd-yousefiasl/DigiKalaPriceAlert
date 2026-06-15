using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Media.AdminSide
{
    public class AdminSideFilterSocialMediaDto : BasePaging<AdminSideSocialMediaDto>
    {
        [Display(Name = "جست و جو")]
        public string? Search { get; set; }

        [Display(Name = "وضعیت انتشار")]
        public bool? IsPublished { get; set; }
    }
}
