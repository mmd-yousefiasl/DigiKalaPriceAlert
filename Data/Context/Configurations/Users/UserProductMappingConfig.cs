using Domain.Enums.Products;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Users
{
    public class UserProductMappingConfig : IEntityTypeConfiguration<UserProductMapping>
    {
        public void Configure(EntityTypeBuilder<UserProductMapping> builder)
        {
            builder.ToTable("UserProductMappings");

            builder.HasKey(up => up.Id);

            builder.HasQueryFilter(up => !up.IsDeleted);

            builder.Property(up => up.IsRead)
                .HasDefaultValue(false);

            builder.Property(up => up.WatchMode)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(WatchMode.AllVariants);

            builder.Property(up => up.NotifyOnPriceChange)
                .HasDefaultValue(true);

            builder.Property(up => up.NotifyOnBuyBoxChange)
                .HasDefaultValue(true);

            builder.Property(up => up.NotifyOnNewSeller)
                .HasDefaultValue(false);

            builder.Property(up => up.NotifyOnSellerLeft)
                .HasDefaultValue(false);

            builder.Property(up => up.MinPriceChangeThreshold)
                .HasColumnType("decimal(18,0)");

            builder.HasIndex(up => new { up.UserId, up.ProductId })
                .IsUnique();

            builder.HasIndex(up => up.UserId);

            builder.HasIndex(up => up.ProductId);

            builder.HasOne(up => up.User)
                .WithMany(u => u.Products)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(up => up.Product)
                .WithMany(p => p.Users)
                .HasForeignKey(up => up.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}