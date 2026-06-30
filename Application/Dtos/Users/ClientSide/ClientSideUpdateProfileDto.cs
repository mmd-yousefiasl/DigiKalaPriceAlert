using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Users.ClientSide
{
    public class ClientSideUpdateProfileDto
    {
        [Display(Name = "نام")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(15, ErrorMessage = Messages.MaxLengthError)]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(100, ErrorMessage = Messages.MaxLengthError)]
        public string LastName { get; set; }
    }
}
