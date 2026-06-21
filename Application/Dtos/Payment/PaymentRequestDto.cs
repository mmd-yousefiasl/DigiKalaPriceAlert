namespace Application.Dtos.Payment
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string CallBackUrl { get; set; }
    }
}
