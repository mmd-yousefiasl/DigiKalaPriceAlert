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
                Price = 990000,
                DurationInDays = 30,
                IsPopular = false,
                ShortDescription = "ویژه فروشندگانی که می‌خواهند بای‌باکس را تصاحب کنند.",
            });

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 3,
                Name = "اشتراک ۹۰ روزه (۳ ماهه)",
                Price = 2675000,
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
                        Title = " پایش همزمان حداکثر ۲ کالا"
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
                        Title = " ارسال هشدار سریع پیامکی به محض تغییر قیمت"
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
                        Title = "ارسال فوری پیامک در صورت تغییر قیمت"
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
                        Title = " ارسال آنی پیامک هشدار تغییر قیمت کالا"
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
        }
    }
}
