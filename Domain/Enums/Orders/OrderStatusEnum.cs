using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.Orders
{
    public enum OrderStatusEnum
    {
        [Display(Name = "پیش‌نویس فاکتور")]
        Draft = 1,

        [Display(Name = "پرداخت موفق و فعال‌شده")]
        Success = 2,

        [Display(Name = "خطا در پرداخت")]
        Failed = 3,

        [Display(Name = "لغو شده توسط کاربر")]
        Canceled = 4
    }
}
