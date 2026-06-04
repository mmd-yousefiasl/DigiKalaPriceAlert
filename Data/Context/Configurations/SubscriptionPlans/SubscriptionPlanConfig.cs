using Domain.Models.SubscriptionPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.SubscriptionPlans
{
    public class SubscriptionPlanConfig : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.ToTable("SubscriptionPlan");

            builder.HasKey(sp => sp.Id);

            builder.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(sp => sp.DisplayPrice)
                .HasColumnType("numeric(18,0)");

            builder.Property(sp => sp.OrderPrice)
                .IsRequired()
               .HasColumnType("numeric(18,0)");
        }
    }
}
