using Application.Dtos.Role.AdminSide;

namespace Application.Dtos.Users.AdminSide
{
    public class AdminSideUserDetailsDto : CommonDto
    {
        public string Mobile { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public decimal Balance { get; set; }
        public bool IsUserBanned { get; set; } = false;
        public List<AdminSideRoleDto>? UserRoles { get; set; }
    }
}
