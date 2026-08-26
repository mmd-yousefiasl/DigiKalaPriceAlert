using Domain.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Products
{
    public class ProductVariantChangeConfig : IEntityTypeConfiguration<ProductVariantChange>
    {
        public void Configure(EntityTypeBuilder<ProductVariantChange> builder)
        {
            builder.ToTable("ProductVariantChanges");

            builder.HasKey(c => c.Id);

            builder.HasQueryFilter(c => !c.IsDeleted);

            builder.Property(c => c.ColorName)
                .IsRequired()
                .HasMaxLength(100);

            // THE read-path index: ProductDetails feed = latest N changes for one color group
            builder.HasIndex(c => new { c.ProductId, c.ColorName, c.CreatedDate });

            builder.HasIndex(c => c.CreatedDate);

            // Items are stored as jsonb — they are only ever rendered with their
            // parent record, never queried independently.
            builder.OwnsMany(c => c.Items, b =>
            {
                b.ToJson();

                b.Property(i => i.DigiKalaSellerId)
                    .HasMaxLength(50);

                b.Property(i => i.SellerName)
                    .HasMaxLength(200);

                b.Property(i => i.OldPrice)
                    .HasColumnType("decimal(18,0)");

                b.Property(i => i.NewPrice)
                    .HasColumnType("decimal(18,0)");

                b.Property(i => i.ChangePercent)
                    .HasColumnType("decimal(18,4)");
            });

            builder.HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
