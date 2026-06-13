using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.NewsLetter.AdminSide
{
    public class AdminSideFilterNewsLettersDto : BasePaging<AdminSideNewsLetterDto>
    {
        [Display(Name = "جست و جو")]
        public string? Search { get; set; }
    }
}
