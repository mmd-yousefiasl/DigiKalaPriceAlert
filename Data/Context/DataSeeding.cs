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

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 1,
                Name = "پلن تست (۷ روزه)",
                Price = 0,
                DurationInDays = 7,
                IsPopular = false,
                ShortDescription = "مناسب برای بررسی و تست سرعت سیستم پایش قیمت.",
            });

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 2,
                Name = "اشتراک ۳۰ روزه (ماهانه)",
                Price = 460000,
                DurationInDays = 30,
                IsPopular = false,
                ShortDescription = "ویژه فروشندگانی که می‌خواهند بای‌باکس را تصاحب کنند.",
            });

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 3,
                Name = "اشتراک ۹۰ روزه (۳ ماهه)",
                Price = 990000,
                DurationInDays = 90,
                IsPopular = true,
                ShortDescription = "بهترین انتخاب برای کاهش هزینه‌ها و تمرکز کامل بر فروش.",
            });

            #endregion

            #region Subscription Plan Features

            var planFeaturesData = new List<SubscriptionPlanFeature>()
                {
                    new SubscriptionPlanFeature()
                    {
                        Id = 1,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 1,
                        IsDeleted =false,
                        Title = " پایش همزمان تا 20 کالا"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 2,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 1,
                        IsDeleted =false,
                        Title = "بررسی مستمر هر ۱۵ دقیقه"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 3,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 1,
                        IsDeleted =false,
                        Title = " ارسال هشدار فوری تلگرامی به محض تغییر قیمت "
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 4,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 1,
                        IsDeleted =false,
                        Title = "حاوی لینک مستقیم محصول در پنل سایت"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 5,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 1,
                        IsDeleted =false,
                        Title = "مدیریت و افزودن آسان کالاها در پنل کاربری"
                    },
                     new SubscriptionPlanFeature()
                    {
                        Id = 6,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 2,
                        IsDeleted =false,
                        Title = "پایش همزمان تا ۲۰ کالا"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 7,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 2,
                        IsDeleted =false,
                        Title = "بررسی مستمر هر ۱۵ دقیقه"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 8,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 2,
                        IsDeleted =false,
                        Title = "ارسال فوری گزارش تلگرامی در صورت تغییر قیمت رقیب "
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 9,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 2,
                        IsDeleted =false,
                        Title = " همراه با لینک مستقیم هدایت به پنل محصول"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 10,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 2,
                        IsDeleted =false,
                        Title = "گزارش متنی روزانه از نوسانات و وضعیت رقبا"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 11,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 2,
                        IsDeleted =false,
                        Title = "پشتیبانی اختصاصی از طریق تیکت سایت"
                    },
                     new SubscriptionPlanFeature()
                    {
                        Id = 12,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 3,
                        IsDeleted =false,
                        Title = "پایش همزمان تا ۲۰ کالا (هر ۱۵ دقیقه)"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 13,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 3,
                        IsDeleted =false,
                        Title = "تخفیف ویژه در خرید سه ماهه"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 14,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 3,
                        IsDeleted =false,
                        Title = "ارسال آنی گزارش تلگرامی تغییر قیمت کالا "
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 15,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 3,
                        IsDeleted =false,
                        Title = "لینک مستقیم ورود به صفحه کالا جهت اصلاح سریع قیمت"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 16,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 3,
                        IsDeleted =false,
                        Title = "مصونیت از افزایش قیمت تعرفه در طول دوره"
                    },
                    new SubscriptionPlanFeature()
                    {
                        Id = 17,
                        IsPublished = true,
                        Priority = 10,
                        SubscriptionPlanId = 3,
                        IsDeleted =false,
                        Title = " اولویت بالا در پردازش کالاها و دریافت آپدیت‌ها"
                    },
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
