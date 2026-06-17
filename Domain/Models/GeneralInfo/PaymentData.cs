namespace Domain.Models.GeneralInfo
{
    public class PaymentData : BaseEntity<int>
    {
        public string PaymentGatewayPin { get; set; }
        public string? FileName { get; set; }
        public string? Url { get; set; }
    }
}
