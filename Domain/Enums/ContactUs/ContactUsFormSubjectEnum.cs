using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.ContactUs
{
    public enum ContactUsFormSubjectEnum
    {
        [Display(Name = "پشتیبانی و راهنمایی کار با ربات")]
        Support,
        [Display(Name = "سوالات مربوط به تعرفه‌ها و اشتراک")]
        SubscriptionSupport,
        [Display(Name = "گزارش مشکل فنی در دریافت اعلان‌ها")]
        ReportProblem,
        [Display(Name = "سایر موارد و پیشنهادات")]
        OtherIssues
    }
}
