using Domain.Models.SubscriptionPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.SubscriptionPlans
{
    public class SubscriptionPlanFeatureConfig : IEntityTypeConfiguration<SubscriptionPlanFeature>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlanFeature> builder)
        {
            builder.ToTable("SubscriptionPlanFeatures");

            builder.HasKey(spf => spf.Id);

            builder.Property(spf => spf.Description)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(spf => spf.Plan)
                .WithMany(sp => sp.Features)
                .HasForeignKey(spf => spf.SubscriptionPlanId);
        }
    }
}
