using Domain.Models.Users;

namespace Domain.Models.SubscriptionPlans
{
    public class SubscriptionPlan : AuditBaseEntity<int>
    {
        public string Name { get; set; }

        public decimal? DisplayPrice { get; set; }
        public decimal OrderPrice { get; set; }
        public int MaximumProductCount { get; set; }

        #region Relations   
        public ICollection<SubscriptionPlanFeature>? Features { get; set; }
        public ICollection<User>? Users { get; set; }
        #endregion
    }
}
