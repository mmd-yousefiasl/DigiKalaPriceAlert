using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Enums.Payment
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VerifyPaymentResultStatusEnum
    {
        [Display(Name = "خطا در تایید تراکنش")]
        Error = 0,

        [Display(Name = "تراکنش با موفقیت تایید شد")]
        Success = 100,

        [Display(Name = "تراکنش قبلاً تایید شده است")]
        AlreadyVerified = 201,

        [Display(Name = "سفارش پرداخت نشده یا ناموفق بوده است")]
        OrderFailedOrUnpaid = 202
    }
}
