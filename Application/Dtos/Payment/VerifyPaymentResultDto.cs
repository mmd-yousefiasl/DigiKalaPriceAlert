using Domain.Enums.Payment;
using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Payment
{
    public class VerifyPaymentResultDto
    {
        public string message { get; set; }

        [Display(Name = "نتیجه تراکنش")]
        [Required(ErrorMessage = Messages.RequiredError)]
        public VerifyPaymentResultStatusEnum result { get; set; }

        [Display(Name = "وضعیت تراکنش")]
        [Required(ErrorMessage = Messages.RequiredError)]
        public PaymentResultStatusEnum status { get; set; }
    }
}
