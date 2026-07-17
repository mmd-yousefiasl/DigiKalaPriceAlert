using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.GeneralInfo
{
    public enum SmsPanelTypeEnum
    {
        [Display(Name = "کد تایید")]
        OTP = 1,

        [Display(Name = "هشدار قیمت")]
        PriceAlert = 2,

        [Display(Name = "هشدار بای باکس")]
        BuyBoxAlert = 3,

        [Display(Name = "هشدار قیمت و بای باکس")]
        PriceAndBuyBoxAlert = 4,
    }
}
