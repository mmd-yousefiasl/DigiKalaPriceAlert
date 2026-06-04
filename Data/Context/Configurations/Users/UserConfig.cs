using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Users
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Mobile)
                .HasMaxLength(11)
                .IsRequired();

            builder.HasIndex(u => u.Mobile)
                .IsUnique();

            builder.Property(u => u.ChatId)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.HasIndex(u => u.ChatId)
                .IsUnique()
                .HasFilter("\"ChatId\" IS NOT NULL");

            builder.Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(u => u.FirstName)
                .HasMaxLength(150)
                .IsRequired(false);

            builder.Property(u => u.LastName)
                .HasMaxLength(150)
                .IsRequired(false);

            builder.HasOne(u => u.SubscriptionPlan)
                .WithMany(sp => sp.Users)
                .HasForeignKey(u => u.SubscriptionPlanId);
        }
    }
}