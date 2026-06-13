using Domain.Enums.ContactUs;

namespace Domain.Models.ContactUs
{
    public class ContactUsFrom : AuditBaseEntity<int>
    {
        public string Name { get; set; }
        public string MobileOrId { get; set; }
        public ContactUsFormSubjectEnum Subject { get; set; }
        public string Message { get; set; }

        public int? UserId { get; set; }
        public string UserIP { get; set; }
    }
}
