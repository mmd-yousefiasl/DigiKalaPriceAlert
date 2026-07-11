using Domain.Models;

namespace Application.Dtos.Blog.ClientSide
{
    public class ClientSideFilterBlogsDto : BasePaging<ClientSideBlogDto>
    {
        public string? CategorySlug { get; set; }
        public string? CategoryTitle { get; set; }
        public string? CategoryMetaDescription { get; set; }
    }
}
