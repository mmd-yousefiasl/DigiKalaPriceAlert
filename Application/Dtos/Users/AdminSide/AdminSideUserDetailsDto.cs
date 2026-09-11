namespace Application.Dtos.Users.AdminSide
{
    public class AdminSideUserDetailsDto : CommonDto
    {
        public int DaysToNotificationSubscriptionEnd { get; set; }
        public int DaysToAutoPriceSubscriptionEnd { get; set; }
        public decimal Balance { get; set; }
        public bool IsUserBanned { get; set; } = false;
    }
}
