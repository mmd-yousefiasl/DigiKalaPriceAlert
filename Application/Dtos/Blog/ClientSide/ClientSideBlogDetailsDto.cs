namespace Application.Dtos.Blog.ClientSide
{
    public class ClientSideBlogDetailsDto : CommonDto
    {
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string MetaDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Writer { get; set; }
        public string Category { get; set; }
        public string CategorySlug { get; set; }
        public int CategoryId { get; set; }
        public string StudyTime { get; set; }
        public string Description { get; set; }
        public int ViewsCount { get; set; }
    }
}