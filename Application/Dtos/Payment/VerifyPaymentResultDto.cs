using Domain.Enums.Payment;

namespace Application.Dtos.Payment
{
    public class VerifyPaymentResultDto
    {
        public string Status { get; set; }
        public VerifyPaymentResultStatusEnum Code { get; set; }
    }
}
