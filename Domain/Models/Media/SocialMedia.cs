namespace Domain.Models.Media
{
    public class SocialMedia : BaseEntity<int>
    {
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public int Priority { get; set; }
        public bool IsPublished { get; set; }
    }
}
