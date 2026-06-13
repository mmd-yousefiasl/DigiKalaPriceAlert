namespace Application.Dtos.Permission.AdminSide
{
    public class AdminSidePermissionDto : CommonDto
    {
        public string Title { get; set; }
        public int? ParentId { get; set; }
    }
}
