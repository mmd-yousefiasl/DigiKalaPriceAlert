using Domain.Enums.Orders;

namespace Application.Dtos.Orders.AdminSide
{
    public class AdminSideOrderDto : CommonDto
    {
        public OrderStatusEnum Status { get; set; }
        public decimal? PlanPrice { get; set; }
        public string PlanTitle { get; set; }
        public DateTime? PaidDate { get; set; }
        public int UserId { get; set; }
        public string? UserMobile { get; set; }
    }
}
