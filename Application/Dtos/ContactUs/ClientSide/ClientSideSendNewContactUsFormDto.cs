using Domain.Enums.ContactUs;
using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.ContactUs.ClientSide
{
    public class ClientSideSendNewContactUsFormDto
    {
        [Display(Name = "نام و نام خانوادگی")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(100, ErrorMessage = Messages.MaxLengthError)]
        public string FullName { get; set; }

        [Display(Name = "شماره موبایل")]
        [MaxLength(15, ErrorMessage = Messages.MaxLengthError)]
        [Required(ErrorMessage = Messages.RequiredError)]
        [RegularExpression("^09\\d{9}$", ErrorMessage = Messages.MobileFormatError)]
        public string Mobile { get; set; }

        [Display(Name = "موضوع پیام")]
        [Required(ErrorMessage = Messages.RequiredError)]
        public ContactUsFormSubjectEnum Subject { get; set; }

        [Display(Name = "متن پیام")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(500, ErrorMessage = Messages.MaxLengthError)]
        public string Message { get; set; }
    }
}
