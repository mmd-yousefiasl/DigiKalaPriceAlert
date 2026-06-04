using Domain.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Products
{
    public class ProductPriceHistoryConfig : IEntityTypeConfiguration<ProductPriceHistory>
    {
        public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
        {
            builder.ToTable("ProductPriceHistory");

            builder.HasKey(pph => pph.Id);

            builder.Property(pph => pph.Price)
                .HasColumnType("numeric(18,0)");

            builder.HasOne(pph => pph.Product)
                .WithMany(p => p.PriceHistory)
                .HasForeignKey(pph => pph.ProductId);
        }
    }
}
