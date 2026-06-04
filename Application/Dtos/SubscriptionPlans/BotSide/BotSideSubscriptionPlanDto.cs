namespace Application.Dtos.SubscriptionPlans.BotSide
{
    public class BotSideSubscriptionPlanDto
    {
        public string Name { get; set; }
        public int MaximumProductCount { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public string StatusDescription { get; set; }
    }
}
