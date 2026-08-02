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
                .HasColumnType("decimal(5,2)");

            builder.Property(s => s.IsBuyBoxWinner)
                .HasDefaultValue(false);

            builder.Property(s => s.WasBuyBoxWinner)
                .HasDefaultValue(false);

            builder.Property(s => s.IsActive)
                .HasDefaultValue(true);

            builder.Property(s => s.DigiKalaVariantId)
                .IsRequired();

            builder.Property(s => s.ColorName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.ColorHex)
                .HasMaxLength(20);

            // UPDATED INDEX: A seller can sell multiple colors of the same product.
            // So the unique constraint must include the VariantId (or ColorName).
            builder.HasIndex(s => new { s.ProductId, s.DigiKalaVariantId, s.DigiKalaSellerId })
                .IsUnique();

            // NEW INDEX: Highly optimizes the "Competitor Lookup" queries 
            // where we filter by ProductId and ColorName to find rivals.
            builder.HasIndex(s => new { s.ProductId, s.ColorName })
                .IncludeProperties(s => new { s.CurrentPrice, s.IsActive, s.IsBuyBoxWinner });

            // INDEX: For finding the buy box winner of a product quickly
            builder.HasIndex(s => new { s.ProductId, s.IsBuyBoxWinner })
                .HasFilter("\"IsBuyBoxWinner\" = true");

            builder.HasIndex(s => s.DigiKalaSellerId);

            builder.HasOne(s => s.Product)
                .WithMany(p => p.Sellers)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
