    using Domain.Models.Products;
    using Domain.Models.SubscriptionPlans;
    using Domain.Models.Users;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;

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

            public DbSet<Product> Products => Set<Product>();
            public DbSet<ProductPriceHistory> ProductPriceHistory => Set<ProductPriceHistory>();


            public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
            public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures => Set<SubscriptionPlanFeature>();
        }
    }
