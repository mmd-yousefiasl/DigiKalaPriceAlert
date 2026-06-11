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
            #region Free Subscription Plan
            modelBuilder.Entity<SubscriptionPlan>().HasData(new SubscriptionPlan()
            {
                Id = 1,
                Name = "پلن رایگان",
                OrderPrice = 0,
                MaximumProductCount = 3,
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
                SubscriptionPlanId = 1,
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
        }
    }
}
