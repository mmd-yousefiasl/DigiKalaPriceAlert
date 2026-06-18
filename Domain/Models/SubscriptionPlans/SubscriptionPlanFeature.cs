namespace Domain.Models.SubscriptionPlans
{
    public class SubscriptionPlanFeature : BaseEntity<int>
    {
        public string Title { get; set; }

        public bool IsPublished { get; set; }
        public int Priority { get; set; }

        #region Relations
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan Plan { get; set; }
        #endregion
    }
}
