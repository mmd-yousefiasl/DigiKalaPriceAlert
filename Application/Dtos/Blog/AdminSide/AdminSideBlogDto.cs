namespace Application.Dtos.Blog.AdminSide
{
    public class AdminSideBlogDto : CommonDto
    {
        public string Title { get; set; }
        public string Writer { get; set; }
        public string StudyTime { get; set; }
        public int Priority { get; set; }
        public bool IsPublished { get; set; }
        public string CategoryTitle { get; set; }
        public int ViewsCount { get; set; }
    }
}
