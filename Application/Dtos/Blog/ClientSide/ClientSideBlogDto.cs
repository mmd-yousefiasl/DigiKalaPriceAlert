namespace Application.Dtos.Blog.ClientSide
{
    public class ClientSideBlogDto : CommonDto
    {
        public DateTime CreatedDate { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
    }
}
