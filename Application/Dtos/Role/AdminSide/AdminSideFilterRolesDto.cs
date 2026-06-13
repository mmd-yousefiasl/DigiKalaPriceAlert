using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Role.AdminSide
{
    public class AdminSideFilterRolesDto : BasePaging<AdminSideRoleDto>
    {
        [Display(Name = "جست و جو")]
        public string? Search { get; set; }
    }
}
