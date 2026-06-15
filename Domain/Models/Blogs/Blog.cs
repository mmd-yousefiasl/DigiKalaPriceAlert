namespace Domain.Models.Blogs
{
    public class Blog : AuditBaseEntity<int>
    {
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Writer { get; set; }
        public string StudyTime { get; set; }
        public int Priority { get; set; }
        public bool IsPublished { get; set; }

        #region Relations
        public int CategoryId { get; set; }
        public BlogCategory Category { get; set; }
        public ICollection<BlogView>? Views { get; set; }
        #endregion
    }
}
