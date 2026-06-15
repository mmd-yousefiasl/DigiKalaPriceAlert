using Domain.Models;

namespace Application.Dtos.Blog.ClientSide
{
    public class ClientSideFilterBlogsDto : BasePaging<ClientSideBlogDto>
    {
        public string? Category { get; set; }
    }
}
