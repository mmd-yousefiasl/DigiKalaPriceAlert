using Domain.Enums.ContactUs;
using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.ContactUs.AdminSide
{
    public class AdminSideFilterContactUsFormsDto : BasePaging<AdminSideContactUsFormDto>
    {
        [Display(Name = "جست و جو")]
        public string? Search { get; set; }

        [Display(Name = "موضوع پیام")]
        public ContactUsFormSubjectEnum? Subject { get; set; }

        [Display(Name = "وضعیت")]
        public bool? IsRead { get; set; }
    }
}
