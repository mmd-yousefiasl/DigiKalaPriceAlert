using Domain.Models.Users;

namespace Domain.Models.Identity
{
    public class Role : AuditBaseEntity<int>
    {
        public string Title { get; set; }
        public string UniqueName { get; set; }

        #region Relations

        public ICollection<UserRoleMapping>? UserRoleMapping { get; set; }
        public ICollection<RolePermissionMapping> RolePermissionMapping { get; set; }

        #endregion
    }
}
