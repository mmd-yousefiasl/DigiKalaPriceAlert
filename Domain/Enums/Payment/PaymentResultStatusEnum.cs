using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.Payment
{
    public enum PaymentResultStatusEnum
    {
        [Display(Name = "در انتظار پرداخت")]
        Pending = -1,

        [Display(Name = "ناموفق / خطا")]
        Failed = 0,

        [Display(Name = "موفق و تایید شده")]
        Succeed = 1,

        [Display(Name = "پرداخت شده - تایید نشده")]
        PaidButNotVerified = 2,

        [Display(Name = "لغو شده توسط کاربر")]
        CanceledByUser = 3,

        [Display(Name = "استرداد شده (Refounded)")]
        Refunded = 15,

        [Display(Name = "در حال استرداد")]
        RefundPending = 16,

        [Display(Name = "برگشت خورده (Reversed)")]
        Reversed = 18
    }
}
