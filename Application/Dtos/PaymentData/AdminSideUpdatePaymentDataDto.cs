using Domain.Shared;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.PaymentData
{
    public class AdminSideUpdatePaymentDataDto
    {
        [Display(Name = "پین درگاه پرداخت")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(500, ErrorMessage = Messages.MaxLengthError)]
        public string PaymentGatewayPin { get; set; }

        [Display(Name = "تصویر")]
        [MaxLength(50, ErrorMessage = Messages.MaxLengthError)]
        public string? FileName { get; set; }

        [Display(Name = "تصویر")]
        public IFormFile? File { get; set; }

        [Display(Name = "لینک")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(1000, ErrorMessage = Messages.MaxLengthError)]
        public string Url { get; set; }
    }
}
