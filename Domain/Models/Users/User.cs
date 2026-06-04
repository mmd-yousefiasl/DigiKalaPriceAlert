using Domain.Models.SubscriptionPlans;

namespace Domain.Models.Users
{
    public class User : AuditBaseEntity<int>
    {
        public string Mobile { get; set; }
        public long? ChatId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }


        #region Relations
        public ICollection<UserProductMapping>? Products { get; set; }
        public int SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        #endregion
    }
}
