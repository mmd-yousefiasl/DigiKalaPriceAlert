namespace Application.Dtos.Faq.AdminSide
{
    public class AdminSideFaqDto : CommonDto
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public int Priority { get; set; }
        public bool IsPublished { get; set; }
    }
}
