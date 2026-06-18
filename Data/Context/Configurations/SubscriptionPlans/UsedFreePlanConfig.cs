using Domain.Models.SubscriptionPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.SubscriptionPlans
{
    public class UsedFreePlanConfig : IEntityTypeConfiguration<UsedFreePlan>
    {
        public void Configure(EntityTypeBuilder<UsedFreePlan> builder)
        {
            builder.ToTable("UsedFreePlans");

            builder.HasKey(ufp => ufp.Id);

            builder.Property(ufp => ufp.Mobile)
                .HasMaxLength(11)
                .IsRequired();

            builder.HasIndex(ufp => ufp.Mobile)
                .IsUnique();
        }
    }
}
