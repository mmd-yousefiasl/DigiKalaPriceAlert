using Domain.Enums.ContactUs;
using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.ContactUs.ClientSide
{
    public class ClientSideSendNewContactUsFormDto
    {
        [Display(Name = "نام و نام خانوادگی / نام فروشگاه")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(100, ErrorMessage = Messages.MaxLengthError)]
        public string Name { get; set; }

        [Display(Name = "شماره موبایل یا آیدی تلگرام")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(100, ErrorMessage = Messages.MaxLengthError)]
        public string MobileOrId { get; set; }

        [Display(Name = "موضوع پیام")]
        [Required(ErrorMessage = Messages.RequiredError)]
        public ContactUsFormSubjectEnum Subject { get; set; }

        [Display(Name = "متن پیام")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(500, ErrorMessage = Messages.MaxLengthError)]
        public string Message { get; set; }
    }
}
