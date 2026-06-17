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
                Name = "تست 7 روزه",
                Price = 0,
                DurationInDays = 7,
            });

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 2,
                Name = "ماهانه",
                Price = 990000,
                DurationInDays = 30,
            });

            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 3,
                Name = "۳ ماهه",
                Price = 2675000,
                DurationInDays = 90,
            });
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
                LastName = null,
                ChatId = null,
                Username = null
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
