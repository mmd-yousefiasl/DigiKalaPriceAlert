using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.Users
{
    public enum UserRegistrationTypeEnum
    {
        [Display(Name = "وبسایت")]
        Website = 1,
        [Display(Name = "ربات بله")]
        BaleBot = 2
    }
}
