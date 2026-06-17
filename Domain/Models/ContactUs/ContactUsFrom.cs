using Domain.Enums.ContactUs;

namespace Domain.Models.ContactUs
{
    public class ContactUsFrom : AuditBaseEntity<int>
    {
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public ContactUsFormSubjectEnum Subject { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public int? ReadByUserId { get; set; }
        public int? UserId { get; set; }
        public string UserIP { get; set; }
    }
}
