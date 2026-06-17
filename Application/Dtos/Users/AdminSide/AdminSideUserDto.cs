namespace Application.Dtos.Users.AdminSide
{
    public class AdminSideUserDto : CommonDto
    {
        public string Mobile { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsUserBanned { get; set; } = false;
    }
}
