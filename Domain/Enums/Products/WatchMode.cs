using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.Products
{
    public enum WatchMode
    {
        [Display(Name = "تمامی محصولات")]
        AllVariants = 1,

        [Display(Name = "محصولات مشخص")]
        SpecificVariants = 2,

        [Display(Name = "فقط محصولات فروشگاه من")]
        MyShopVariantsOnly = 3
    }
}