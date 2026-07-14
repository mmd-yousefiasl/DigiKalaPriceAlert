using Domain.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Products
{
    public class ProductVariantConfig : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");

            builder.HasKey(v => v.Id);

            builder.HasQueryFilter(v => !v.IsDeleted);

            builder.Property(v => v.DigiKalaVariantId)
                .IsRequired();

            builder.Property(v => v.ColorName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.ColorHex)
                .HasMaxLength(20);

            builder.Property(v => v.SizeLabel)
                .HasMaxLength(50);

            builder.Property(v => v.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(v => new { v.ProductId, v.DigiKalaVariantId })
                .IsUnique();

            builder.HasIndex(v => v.ColorName);

            builder.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
