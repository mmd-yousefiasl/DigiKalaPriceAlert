using Domain.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Products
{
    public class ProductVariantSellerConfig : IEntityTypeConfiguration<ProductVariantSeller>
    {
        public void Configure(EntityTypeBuilder<ProductVariantSeller> builder)
        {
            builder.ToTable("ProductVariantSellers");

            builder.HasKey(s => s.Id);

            builder.HasQueryFilter(s => !s.IsDeleted);

            builder.Property(s => s.DigiKalaSellerId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(s => s.SellerName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.CurrentPrice)
                .IsRequired()
                .HasColumnType("decimal(18,0)");

            builder.Property(s => s.PreviousPrice)
                .HasColumnType("decimal(18,0)");

            builder.Property(s => s.SellerRating)
                .HasColumnType("decimal(4,2)");

            builder.Property(s => s.IsBuyBoxWinner)
                .HasDefaultValue(false);

            builder.Property(s => s.WasBuyBoxWinner)
                .HasDefaultValue(false);

            builder.Property(s => s.IsActive)
                .HasDefaultValue(true);

            builder.HasIndex(s => new { s.ProductVariantId, s.DigiKalaSellerId })
                .IsUnique();

            builder.HasIndex(s => new { s.ProductVariantId, s.IsBuyBoxWinner })
                .HasFilter("\"IsBuyBoxWinner\" = true");

            builder.HasIndex(s => s.DigiKalaSellerId);

            builder.HasOne(s => s.Variant)
                .WithMany(v => v.Sellers)
                .HasForeignKey(s => s.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
