namespace Application.Dtos.Orders.ClientSide
{
    public class ClientSideInvoiceDto : CommonDto
    {
        public decimal? PlanPrice { get; set; }
        public string? PlanTitle { get; set; }
        public string? ShortDescription { get; set; }
        public bool IsPopular { get; set; }
        public int DurationInDays { get; set; }
        public List<string>? Features { get; set; }
        public decimal UserBalance { get; set; }
    }
}
