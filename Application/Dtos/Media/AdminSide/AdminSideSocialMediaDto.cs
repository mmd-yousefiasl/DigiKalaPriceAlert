namespace Application.Dtos.Media.AdminSide
{
    public class AdminSideSocialMediaDto : CommonDto
    {
        public string Title { get; set; }
        public int Priority { get; set; }
        public bool IsPublished { get; set; }
    }
}
