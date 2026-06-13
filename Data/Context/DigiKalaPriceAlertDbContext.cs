using Domain.Models.ContactUs;
using Domain.Models.Identity;
using Domain.Models.Products;
using Domain.Models.SubscriptionPlans;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class DigiKalaPriceAlertDbContext : DbContext
    {
        public DigiKalaPriceAlertDbContext(DbContextOptions<DigiKalaPriceAlertDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DigiKalaPriceAlertDbContext).Assembly);
            modelBuilder.Seed();
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserProductMapping> UserProductMapping => Set<UserProductMapping>();
        public DbSet<BalanceTransaction> BalanceTransactions => Set<BalanceTransaction>();
        public DbSet<UserRoleMapping> UserRoleMapping => Set<UserRoleMapping>();
        public DbSet<UserValidation> UserValidation => Set<UserValidation>();

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductPriceHistory> ProductPriceHistory => Set<ProductPriceHistory>();


        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures => Set<SubscriptionPlanFeature>();

        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RolePermissionMapping> RolePermissionMapping => Set<RolePermissionMapping>();

        public DbSet<ContactUsFrom> ContactUsForms => Set<ContactUsFrom>();
    }
}
