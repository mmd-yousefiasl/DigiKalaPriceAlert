namespace Domain.Models.Users
{
    public class TelegramValidation : AuditBaseEntity<int>
    {
        public int UserId { get; set; }
        public string Code { get; set; }
    }
}
