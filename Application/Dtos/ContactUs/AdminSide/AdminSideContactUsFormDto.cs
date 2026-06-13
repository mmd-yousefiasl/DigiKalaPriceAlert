using Domain.Enums.ContactUs;

namespace Application.Dtos.ContactUs.AdminSide
{
    public class AdminSideContactUsFormDto : CommonDto
    {
        public DateTime CreatedDate { get; set; }
        public string Name { get; set; }
        public string MobileOrId { get; set; }
        public ContactUsFormSubjectEnum Subject { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public int? UserId { get; set; }
        public string UserIP { get; set; }
    }
}