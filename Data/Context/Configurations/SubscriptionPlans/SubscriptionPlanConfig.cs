using Domain.Models.SubscriptionPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.SubscriptionPlans
{
    public class SubscriptionPlanConfig : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.ToTable("SubscriptionPlans");

            builder.HasKey(sp => sp.Id);

            builder.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sp => sp.ShortDescription)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(sp => sp.Price)
                .HasColumnType("numeric(18,0)")
                .IsRequired();
        }
    }
}
