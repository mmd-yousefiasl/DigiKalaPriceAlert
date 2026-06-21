namespace Application.Dtos.Orders.ClientSide
{
    public class ClientSideOrderDto : CommonDto
    {
        public decimal? PlanPrice { get; set; }
        public string? PlanTitle { get; set; }
        public DateTime? PaidDate { get; set; }
    }
}
