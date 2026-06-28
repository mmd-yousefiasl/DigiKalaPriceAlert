using System.ComponentModel.DataAnnotations;

namespace Domain.Enums.GeneralInfo
{
    public enum SiteMapTypeEnum
    {
        [Display(Name = "دسته بندی مقالات")]
        BlogCategory = 1,

        [Display(Name = "مقالات")]
        Blog = 2,

        [Display(Name = "سایر صفحات")]
        OtherPages = 3
    }
}
