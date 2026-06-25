using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.Products
{
    public enum ProductSortTypeEnum
    {
        [Display(Name = "جدیدترین بروزرسانی")]
        NewestUpdate = 1,

        [Display(Name = "قدیمی‌ترین بروزرسانی")]
        OldestUpdate = 2,

        [Display(Name = "گران‌ترین")]
        HighestPrice = 3,

        [Display(Name = "ارزان‌ترین")]
        LowestPrice = 4,

        [Display(Name = "بیشترین کاهش قیمت (%)")]
        HighestPriceDrop = 5,

        [Display(Name = "فقط خوانده‌نشده‌ها (جدید)")]
        UnreadOnly = 6
    }
}
