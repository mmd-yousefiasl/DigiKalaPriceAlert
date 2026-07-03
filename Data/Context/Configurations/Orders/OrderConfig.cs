using Domain.Models.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Orders
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.PlanPrice)
                .HasColumnType("decimal(18,0)")
                .IsRequired(false);

            builder.Property(o => o.PlanTitle)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(o => o.InvoiceId)
                .HasMaxLength(15)
                .IsRequired(true);

            builder.HasIndex(o => o.InvoiceId)
                .HasFilter("\"InvoiceId\" IS NOT NULL")
                .IsUnique();

            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .IsRequired(false);

            builder.HasOne(o => o.SubscriptionPlan)
                .WithMany(sp => sp.Orders)
                .HasForeignKey(o => o.SubscriptionPlanId);
        }
    }
}
