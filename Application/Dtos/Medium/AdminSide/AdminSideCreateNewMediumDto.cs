using Domain.Shared;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Medium.AdminSide
{
    public class AdminSideCreateNewMediumDto
    {
        [Display(Name = "تصویر")]
        [Required(ErrorMessage = Messages.RequiredError)]
        public IFormFile File { get; set; }
    }
}
