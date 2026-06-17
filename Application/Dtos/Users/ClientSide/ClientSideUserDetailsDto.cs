using Application.Dtos.Role.AdminSide;

namespace Application.Dtos.Users.ClientSide
{
    public class ClientSideUserDetailsDto
    {
        public string Mobile { get; set; }
        public long? ChatId { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public string SubscriptionPlanName { get; set; }
        public int SubscriptionPlanId { get; set; }
        public decimal Balance { get; set; }
        public bool IsUserBanned { get; set; } = false;
        public List<AdminSideRoleDto>? UserRoles { get; set; }
    }
}
