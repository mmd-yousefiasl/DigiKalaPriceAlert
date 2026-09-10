using Domain.Enums.SubscriptionPlans;

namespace Domain.Models.SubscriptionPlans
{
    public class UsedFreePlan : AuditBaseEntity<int>
    {
        public string Mobile { get; set; }
        public int UserId { get; set; }
        public UsedFreePlanTypeEnum Type { get; set; }
    }
}
