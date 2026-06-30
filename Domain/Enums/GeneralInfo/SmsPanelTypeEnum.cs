using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.GeneralInfo
{
    public enum SmsPanelTypeEnum
    {
        [Display(Name = "کد تایید")]
        OTP = 1,

        [Display(Name = "هشدار قیمت")]
        PriceAlert = 2,
    }
}
