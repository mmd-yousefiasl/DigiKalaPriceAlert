using Domain.Models.Orders;

namespace Domain.Models.SubscriptionPlans
{
    public class SubscriptionPlan : AuditBaseEntity<int>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }

        #region Relations
        public ICollection<Order>? Orders { get; set; }
        #endregion
    }
}
