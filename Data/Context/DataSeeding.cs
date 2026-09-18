using Domain.Enums.GeneralInfo;
using Domain.Models.GeneralInfo;
using Domain.Models.Identity;
using Domain.Models.SubscriptionPlans;
using Domain.Models.Users;
using Domain.Statics;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public static class DataSeeding
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            #region Subscription Plans

            // ==================== NOTIFICATION SERVICE (اعلان تغییر قیمت) ====================

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 1,
                Name = "اعلان تغییر قیمت - تست رایگان ۷ روزه",
                Price = 0,
                DurationInDays = 7,
                IsPopular = false,
                ShortDescription = "پایش قیمت رقبا و دریافت هشدار فوری تلگرامی، به‌صورت رایگان.",
            });

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 2,
                Name = "اعلان تغییر قیمت - اشتراک ماهانه ۳۰ روزه",
                Price = 460000,
                DurationInDays = 30,
                IsPopular = false,
                ShortDescription = "مناسب فروشندگانی که می‌خواهند همیشه از تغییر قیمت رقیب باخبر باشند.",
            });

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 3,
                Name = "اعلان تغییر قیمت - اشتراک سه‌ماهه ۹۰ روزه",
                Price = 990000,
                DurationInDays = 90,
                IsPopular = true,
                ShortDescription = "صرفه‌جویی ۲۸٪ نسبت به خرید ماهانه، با قفل قیمت در طول دوره.",
            });

            // ==================== AUTO-PRICING SERVICE (قیمت‌گذاری خودکار) ====================

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 4,
                Name = "قیمت‌گذاری خودکار - اشتراک ماهانه ۳۰ روزه",
                Price = 690000,
                DurationInDays = 30,
                IsPopular = false,
                ShortDescription = "تنظیم خودکار قیمت برای حفظ بای‌باکس، بدون دخالت دستی.",
            });

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 5,
                Name = "قیمت‌گذاری خودکار - اشتراک سه‌ماهه ۹۰ روزه",
                Price = 1590000,
                DurationInDays = 90,
                IsPopular = true,
                ShortDescription = "صرفه‌جویی ۲۳٪ و پشتیبانی اولویت‌دار، برای فروشندگان جدی.",
            });

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 6,
                Name = "قیمت‌گذاری خودکار - تست رایگان ۷ روزه",
                Price = 0,
                DurationInDays = 7,
                IsPopular = false,
                ShortDescription = "بررسی عملکرد سیستم قیمت‌گذاری خودکار، بدون نیاز به کارت بانکی.",
            });

            #endregion

            #region Subscription Plan Features

            var planFeaturesData = new List<SubscriptionPlanFeature>()
{
    // ==================== Plan 1: Notification Trial ====================
    new SubscriptionPlanFeature { Id = 1,  IsPublished = true, Priority = 10, SubscriptionPlanId = 1, IsDeleted = false,
        Title = "پایش همزمان قیمت تا ۲۰ کالا" },
    new SubscriptionPlanFeature { Id = 2,  IsPublished = true, Priority = 10, SubscriptionPlanId = 1, IsDeleted = false,
        Title = "بررسی قیمت رقبا هر ۱۵ دقیقه" },
    new SubscriptionPlanFeature { Id = 3,  IsPublished = true, Priority = 10, SubscriptionPlanId = 1, IsDeleted = false,
        Title = "هشدار فوری پیامرسان بله در لحظه تغییر قیمت" },
    new SubscriptionPlanFeature { Id = 4,  IsPublished = true, Priority = 10, SubscriptionPlanId = 1, IsDeleted = false,
        Title = "لینک مستقیم به صفحه کالا در دیجی‌کالا" },
    new SubscriptionPlanFeature { Id = 5,  IsPublished = true, Priority = 10, SubscriptionPlanId = 1, IsDeleted = false,
        Title = "مدیریت آسان کالاها از پنل کاربری" },
    new SubscriptionPlanFeature { Id = 6,  IsPublished = true, Priority = 10, SubscriptionPlanId = 1, IsDeleted = false,
        Title = "گزارش روزانه نوسانات بازار" },

    // ==================== Plan 2: Notification 30 Days ====================
    new SubscriptionPlanFeature { Id = 7,  IsPublished = true, Priority = 10, SubscriptionPlanId = 2, IsDeleted = false,
        Title = "پایش همزمان قیمت تا ۲۰ کالا" },
    new SubscriptionPlanFeature { Id = 8,  IsPublished = true, Priority = 10, SubscriptionPlanId = 2, IsDeleted = false,
        Title = "بررسی قیمت رقبا هر ۱۵ دقیقه" },
    new SubscriptionPlanFeature { Id = 9,  IsPublished = true, Priority = 10, SubscriptionPlanId = 2, IsDeleted = false,
        Title = "هشدار فوری پیامرسان بله در لحظه تغییر قیمت" },
    new SubscriptionPlanFeature { Id = 10, IsPublished = true, Priority = 10, SubscriptionPlanId = 2, IsDeleted = false,
        Title = "لینک مستقیم به صفحه کالا در دیجی‌کالا" },
    new SubscriptionPlanFeature { Id = 11, IsPublished = true, Priority = 10, SubscriptionPlanId = 2, IsDeleted = false,
        Title = "گزارش روزانه نوسانات و وضعیت رقبا" },
    new SubscriptionPlanFeature { Id = 12, IsPublished = true, Priority = 10, SubscriptionPlanId = 2, IsDeleted = false,
        Title = "پشتیبانی از طریق تیکت" },

    // ==================== Plan 3: Notification 90 Days ====================
    new SubscriptionPlanFeature { Id = 13, IsPublished = true, Priority = 10, SubscriptionPlanId = 3, IsDeleted = false,
        Title = "پایش همزمان قیمت تا ۲۰ کالا" },
    new SubscriptionPlanFeature { Id = 14, IsPublished = true, Priority = 10, SubscriptionPlanId = 3, IsDeleted = false,
        Title = "بررسی قیمت رقبا هر ۱۵ دقیقه" },
    new SubscriptionPlanFeature { Id = 15, IsPublished = true, Priority = 10, SubscriptionPlanId = 3, IsDeleted = false,
        Title = "هشدار فوری پیامرسان بله در لحظه تغییر قیمت" },
    new SubscriptionPlanFeature { Id = 16, IsPublished = true, Priority = 10, SubscriptionPlanId = 3, IsDeleted = false,
        Title = "لینک مستقیم به صفحه کالا در دیجی‌کالا" },
    new SubscriptionPlanFeature { Id = 17, IsPublished = true, Priority = 10, SubscriptionPlanId = 3, IsDeleted = false,
        Title = "گزارش روزانه نوسانات و وضعیت رقبا" },
    new SubscriptionPlanFeature { Id = 18, IsPublished = true, Priority = 10, SubscriptionPlanId = 3, IsDeleted = false,
        Title = "قفل قیمت؛ مصونیت از افزایش تعرفه در طول دوره" },
    new SubscriptionPlanFeature { Id = 19, IsPublished = true, Priority = 10, SubscriptionPlanId = 3, IsDeleted = false,
        Title = "پشتیبانی اولویت‌دار" },
    new SubscriptionPlanFeature { Id = 20, IsPublished = true, Priority = 10, SubscriptionPlanId = 3, IsDeleted = false,
        Title = "اولویت بالاتر در پردازش و ارسال هشدارها" },

    // ==================== Plan 4: Auto-Pricing 30 Days ====================
    new SubscriptionPlanFeature { Id = 21, IsPublished = true, Priority = 10, SubscriptionPlanId = 4, IsDeleted = false,
        Title = "تنظیم خودکار قیمت همه کالاهای پنل" },
    new SubscriptionPlanFeature { Id = 22, IsPublished = true, Priority = 10, SubscriptionPlanId = 4, IsDeleted = false,
        Title = "تعیین کف قیمت (حداقل قیمت مجاز) برای هر کالا" },
    new SubscriptionPlanFeature { Id = 23, IsPublished = true, Priority = 10, SubscriptionPlanId = 4, IsDeleted = false,
        Title = "تنظیم خودکار بر اساس قیمت پایین‌ترین رقیب" },
    new SubscriptionPlanFeature { Id = 24, IsPublished = true, Priority = 10, SubscriptionPlanId = 4, IsDeleted = false,
        Title = "رصد قیمت رقبا هر ۱۵ دقیقه" },
    new SubscriptionPlanFeature { Id = 25, IsPublished = true, Priority = 10, SubscriptionPlanId = 4, IsDeleted = false,
        Title = "گزارش هر تغییر قیمت در پیامرسان بله" },
    new SubscriptionPlanFeature { Id = 26, IsPublished = true, Priority = 10, SubscriptionPlanId = 4, IsDeleted = false,
        Title = "توقف اضطراری در صورت خطا یا نوسان غیرعادی" },
    new SubscriptionPlanFeature { Id = 27, IsPublished = true, Priority = 10, SubscriptionPlanId = 4, IsDeleted = false,
        Title = "پشتیبانی از طریق تیکت" },

    // ==================== Plan 5: Auto-Pricing 90 Days ====================
    new SubscriptionPlanFeature { Id = 28, IsPublished = true, Priority = 10, SubscriptionPlanId = 5, IsDeleted = false,
        Title = "تنظیم خودکار قیمت همه کالاهای پنل" },
    new SubscriptionPlanFeature { Id = 29, IsPublished = true, Priority = 10, SubscriptionPlanId = 5, IsDeleted = false,
        Title = "تعیین کف قیمت (حداقل قیمت مجاز) برای هر کالا" },
    new SubscriptionPlanFeature { Id = 30, IsPublished = true, Priority = 10, SubscriptionPlanId = 5, IsDeleted = false,
        Title = "تنظیم خودکار بر اساس قیمت پایین‌ترین رقیب" },
    new SubscriptionPlanFeature { Id = 31, IsPublished = true, Priority = 10, SubscriptionPlanId = 5, IsDeleted = false,
        Title = "رصد قیمت رقبا هر ۱۵ دقیقه" },
    new SubscriptionPlanFeature { Id = 32, IsPublished = true, Priority = 10, SubscriptionPlanId = 5, IsDeleted = false,
        Title = "گزارش هر تغییر قیمت در پیامرسان بله" },
    new SubscriptionPlanFeature { Id = 33, IsPublished = true, Priority = 10, SubscriptionPlanId = 5, IsDeleted = false,
        Title = "توقف اضطراری در صورت خطا یا نوسان غیرعادی" },
    new SubscriptionPlanFeature { Id = 34, IsPublished = true, Priority = 10, SubscriptionPlanId = 5, IsDeleted = false,
        Title = "قفل قیمت؛ مصونیت از افزایش تعرفه در طول دوره" },
    new SubscriptionPlanFeature { Id = 35, IsPublished = true, Priority = 10, SubscriptionPlanId = 5, IsDeleted = false,
        Title = "پشتیبانی اولویت‌دار" },
    new SubscriptionPlanFeature { Id = 36, IsPublished = true, Priority = 10, SubscriptionPlanId = 5, IsDeleted = false,
        Title = "اولویت بالاتر در پردازش کالاها و دریافت آپدیت‌ها" },

    // ==================== Plan 6: Auto-Pricing Trial ====================
    new SubscriptionPlanFeature { Id = 37, IsPublished = true, Priority = 10, SubscriptionPlanId = 6, IsDeleted = false,
        Title = "تنظیم خودکار قیمت تا ۵ کالا" },
    new SubscriptionPlanFeature { Id = 38, IsPublished = true, Priority = 10, SubscriptionPlanId = 6, IsDeleted = false,
        Title = "تعیین کف قیمت (حداقل قیمت مجاز) برای هر کالا" },
    new SubscriptionPlanFeature { Id = 39, IsPublished = true, Priority = 10, SubscriptionPlanId = 6, IsDeleted = false,
        Title = "رصد قیمت رقبا هر ۳۰ دقیقه" },
    new SubscriptionPlanFeature { Id = 40, IsPublished = true, Priority = 10, SubscriptionPlanId = 6, IsDeleted = false,
        Title = "گزارش هر تغییر قیمت در پیامرسان بله" },
};

            foreach (var item in planFeaturesData)
            {
                modelBuilder.Entity<SubscriptionPlanFeature>().HasData(item);
            }
            #endregion

            #region Permission

            modelBuilder.Entity<Permission>().HasData(PermissionsList.Permissions);

            #endregion

            #region Identity 

            var adminRole = new Role()
            {
                Id = 1,
                Title = "Admin",
                UniqueName = "Admin"
            };
            modelBuilder.Entity<Role>().HasData(adminRole);

            var rolePermissionMapping = new RolePermissionMapping()
            {
                RoleId = 1,
                PermissionId = 1
            };
            modelBuilder.Entity<RolePermissionMapping>().HasData(rolePermissionMapping);

            #endregion

            #region User

            User AdminUser = new User()
            {
                Id = 1,
                Mobile = "09962144266",
                FirstName = null,
                LastName = null
            };

            modelBuilder.Entity<User>().HasData(AdminUser);

            #endregion

            #region UserRoleMapping

            UserRoleMapping adminUserRoleMapping = new UserRoleMapping
            {
                UserId = 1,
                RoleId = 1
            };

            modelBuilder.Entity<UserRoleMapping>().HasData(adminUserRoleMapping);

            #endregion

            #region EmailData
            EmailData emailData = new EmailData()
            {
                Id = 1,
            };

            modelBuilder.Entity<EmailData>().HasData(emailData);
            #endregion

            #region PaymentData
            PaymentData paymentData = new PaymentData()
            {
                Id = 1,
                IsDeleted = false,
                PaymentGatewayPin = "sandbox"
            };
            modelBuilder.Entity<PaymentData>().HasData(paymentData);
            #endregion

            #region SiteMap
            string SiteUrl = "https://digipricealert.ir/";

            List<SiteMap> siteMaps = new List<SiteMap>() {
            new SiteMap()
{
    Id = 1,
    Url = $"{SiteUrl}",
    ChangeFreq = ChangeFreqEnum.Daily,
    Priority = 1.0m,
    IsPublished = true,
    Type = SiteMapTypeEnum.OtherPages,
    IsDeleted = false
},
new SiteMap()
{
    Id = 2,
    Url = $"{SiteUrl}subscription/pricing",
    ChangeFreq = ChangeFreqEnum.Weekly,
    Priority = 0.9m,
    IsPublished = true,
    Type = SiteMapTypeEnum.OtherPages,
    IsDeleted = false
},
new SiteMap()
{
    Id = 3,
    Url = $"{SiteUrl}features",
    ChangeFreq = ChangeFreqEnum.Weekly,
    Priority = 0.8m,
    IsPublished = true,
    Type = SiteMapTypeEnum.OtherPages,
    IsDeleted = false
},
new SiteMap()
{
    Id = 4,
    Url = $"{SiteUrl}faq",
    ChangeFreq = ChangeFreqEnum.Monthly,
    Priority = 0.6m,
    IsPublished = true,
    Type = SiteMapTypeEnum.OtherPages,
    IsDeleted = false
},
new SiteMap()
{
    Id = 5,
    Url = $"{SiteUrl}about-us",
    ChangeFreq = ChangeFreqEnum.Monthly,
    Priority = 0.5m,
    IsPublished = true,
    Type = SiteMapTypeEnum.OtherPages,
    IsDeleted = false
},
new SiteMap()
{
    Id = 6,
    Url = $"{SiteUrl}contact-us",
    ChangeFreq = ChangeFreqEnum.Monthly,
    Priority = 0.5m,
    IsPublished = true,
    Type = SiteMapTypeEnum.OtherPages,
    IsDeleted = false
},
new SiteMap()
{
    Id = 7,
    Url = $"{SiteUrl}blogs",
    ChangeFreq = ChangeFreqEnum.Daily,
    Priority = 0.9m,
    IsPublished = true,
    Type = SiteMapTypeEnum.OtherPages,
    IsDeleted = false
},
new SiteMap()
{
    Id = 8,
    Url = $"{SiteUrl}calculator",
    ChangeFreq = ChangeFreqEnum.Weekly,
    Priority = 0.9m,
    IsPublished = true,
    Type = SiteMapTypeEnum.OtherPages,
    IsDeleted = false
}};

            modelBuilder.Entity<SiteMap>().HasData(siteMaps);
            #endregion
        }
    }
}
