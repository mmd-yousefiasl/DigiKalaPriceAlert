namespace Application.Dtos.Users.BotSide
{
    public class BotSideCreateNewUserDto
    {
        public string Mobile { get; set; }
        public long ChatId { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
