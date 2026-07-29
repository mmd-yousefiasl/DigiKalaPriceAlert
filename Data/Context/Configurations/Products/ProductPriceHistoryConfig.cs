using Domain.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Products
{
    public class ProductPriceHistoryConfig : IEntityTypeConfiguration<ProductPriceHistory>
    {
        public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
        {
            builder.HasKey(ph => ph.Id);

            builder.HasQueryFilter(ph => !ph.IsDeleted);

            builder.Property(ph => ph.OldPrice)
                .IsRequired()
                .HasColumnType("decimal(18,0)");

            builder.Property(ph => ph.NewPrice)
                .IsRequired()
                .HasColumnType("decimal(18,0)");

            builder.Property(ph => ph.ChangePercent)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            builder.Property(ph => ph.ChangeDescription)
                .HasMaxLength(500);

            builder.Property(ph => ph.BuyBoxChanged)
                .HasDefaultValue(false);

            builder.Property(ph => ph.IsBuyBoxWinner)
                .HasDefaultValue(false);

            builder.HasIndex(ph => new { ph.ProductVariantSellerId, ph.CreatedDate });

            builder.HasIndex(ph => ph.CreatedDate);

            builder.HasIndex(ph => ph.BuyBoxChanged)
                .HasFilter("\"BuyBoxChanged\" = true");

            builder.HasOne(ph => ph.VariantSeller)
                .WithMany(vs => vs.PriceHistory)
                .HasForeignKey(ph => ph.ProductVariantSellerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}