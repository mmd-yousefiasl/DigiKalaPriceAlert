namespace Application.Dtos.Payment
{
    public class PaymentRequestDto
    {
        public long Amount { get; set; }
        public string CallBackUrl { get; set; }
    }
}
