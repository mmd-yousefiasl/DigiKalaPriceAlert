using Domain.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Products
{
    public class UserVariantWatchConfig : IEntityTypeConfiguration<UserVariantWatch>
    {
        public void Configure(EntityTypeBuilder<UserVariantWatch> builder)
        {
            builder.ToTable("UserVariantWatches");

            builder.HasQueryFilter(vw => !vw.IsDeleted);

            builder.HasKey(vw => vw.Id);

            builder.Property(vw => vw.UserOwnSellerIdOnVariant)
                .HasMaxLength(50);

            builder.HasIndex(vw => new { vw.UserProductMappingId, vw.ProductVariantId })
                .IsUnique();

            builder.HasIndex(vw => new { vw.ProductVariantId, vw.UserOwnSellerIdOnVariant });

            builder.HasOne(vw => vw.ProductMapping)
                .WithMany(pm => pm.VariantWatches)
                .HasForeignKey(vw => vw.UserProductMappingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(vw => vw.Variant)
                .WithMany(pv => pv.UserWatches)
                .HasForeignKey(vw => vw.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
