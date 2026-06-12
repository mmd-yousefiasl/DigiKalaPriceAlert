using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Users.AdminSide
{
    public class AdminSideUpdateUserDto : CommonDto
    {
        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(11)]
        public string Mobile { get; set; }

        [Display(Name = "نام")]
        [MaxLength(150, ErrorMessage = Messages.MaxLengthError)]
        public string? FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [MaxLength(150, ErrorMessage = Messages.MaxLengthError)]
        public string? LastName { get; set; }

        [Display(Name = "وضعیت حساب کاربری")]
        [Required(ErrorMessage = Messages.RequiredError)]
        public bool IsUserBanned { get; set; }

        [Display(Name = "نقش")]
        public List<int>? RoleIds { get; set; }
    }
}
