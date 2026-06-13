using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Role.AdminSide
{
    public class AdminSideCreateNewRoleDto
    {
        [Display(Name = "عنوان")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(100, ErrorMessage = Messages.MaxLengthError)]
        public string Title { get; set; }

        [Display(Name = "نام یکتا")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(100, ErrorMessage = Messages.MaxLengthError)]
        public string UniqueName { get; set; }

        [Display(Name = "دسترسی ها")]
        [Required(ErrorMessage = Messages.RequiredAccessForRoleError)]
        public List<int> PermissionIds { get; set; }
    }
}
