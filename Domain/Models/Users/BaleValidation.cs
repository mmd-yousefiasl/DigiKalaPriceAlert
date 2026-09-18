namespace Domain.Models.Users
{
    public class BaleValidation : AuditBaseEntity<int>
    {
        public int UserId { get; set; }
        public string Code { get; set; }
    }
}
