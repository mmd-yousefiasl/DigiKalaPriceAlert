using Domain.Models.Blogs;
using Domain.Models.ContactUs;
using Domain.Models.Faqs;
using Domain.Models.GeneralInfo;
using Domain.Models.Identity;
using Domain.Models.Media;
using Domain.Models.NewsLetters;
using Domain.Models.Orders;
using Domain.Models.Products;
using Domain.Models.SiteNotifications;
using Domain.Models.SubscriptionPlans;
using Domain.Models.Tickets;
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
        public DbSet<UserProductMapping> UserProductMappings => Set<UserProductMapping>();
        public DbSet<BalanceTransaction> BalanceTransactions => Set<BalanceTransaction>();
        public DbSet<UserRoleMapping> UserRoleMapping => Set<UserRoleMapping>();
        public DbSet<UserValidation> UserValidation => Set<UserValidation>();

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductPriceHistory> ProductPriceHistory => Set<ProductPriceHistory>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductVariantSeller> ProductVariantSellers => Set<ProductVariantSeller>();


        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures => Set<SubscriptionPlanFeature>();
        public DbSet<UsedFreePlan> UsedFreePlans => Set<UsedFreePlan>();

        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<RolePermissionMapping> RolePermissionMapping => Set<RolePermissionMapping>();

        public DbSet<ContactUsFrom> ContactUsForms => Set<ContactUsFrom>();

        public DbSet<NewsLetter> NewsLetters => Set<NewsLetter>();
        public DbSet<SentLetter> SentLetters => Set<SentLetter>();

        public DbSet<EmailData> EmailData => Set<EmailData>();
        public DbSet<PaymentData> PaymentData => Set<PaymentData>();
        public DbSet<SiteMap> SiteMap => Set<SiteMap>();
        public DbSet<SmsPanel> SmsPanels => Set<SmsPanel>();

        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
        public DbSet<BlogView> BlogViews => Set<BlogView>();

        public DbSet<Faq> Faqs => Set<Faq>();

        public DbSet<SocialMedia> SocialMedia => Set<SocialMedia>();
        public DbSet<Medium> Media => Set<Medium>();

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
        public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();

        public DbSet<SiteNotification> SiteNotifications => Set<SiteNotification>();
    }
}
