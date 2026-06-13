namespace Application.Dtos.SendEmail.AdminSide
{
    public class AdminSideSendBuckEmailDto
    {
        public List<string> Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
