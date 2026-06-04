namespace Domain.Models.SubscriptionPlans
{
    public class SubscriptionPlanFeature : BaseEntity<int>
    {
        public string Description { get; set; }

        #region Relations
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan Plan { get; set; }
        #endregion
    }
}
