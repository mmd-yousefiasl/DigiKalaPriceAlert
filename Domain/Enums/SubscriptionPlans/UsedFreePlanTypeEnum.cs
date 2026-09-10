using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.SubscriptionPlans
{
    public enum UsedFreePlanTypeEnum
    {
        [Display(Name = "اعلان تغییرات")]
        Notification = 1,
        [Display(Name = "قیمت گذاری")]
        AutoPricing = 2
    }
}
