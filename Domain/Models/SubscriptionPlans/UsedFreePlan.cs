namespace Domain.Models.SubscriptionPlans
{
    public class UsedFreePlan : AuditBaseEntity<int>
    {
        public string Mobile { get; set; }
        public int UserId { get; set; }
    }
}
