using Domain.Models.SubscriptionPlans;
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
        }
    }
}
