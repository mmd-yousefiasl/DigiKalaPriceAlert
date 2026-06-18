using Domain.Enums.Orders;
using Domain.Models.SubscriptionPlans;
using Domain.Models.Users;

namespace Domain.Models.Orders
{
    public class Order : AuditBaseEntity<int>
    {
        public OrderStatusEnum Status { get; set; }
        public decimal? PlanPrice { get; set; }
        public string? PlanTitle { get; set; }
        public DateTime? PaidDate { get; set; }

        #region Relations
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        #endregion
    }
}
