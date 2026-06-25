using Domain.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Products
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.DigiKalaId)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(p => p.DigiKalaId)
                .IsUnique();

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.OldPrice)
                .HasColumnType("numeric(18,0)");

            builder.Property(p => p.NewPrice)
                .HasColumnType("numeric(18,0)");

            builder.Property(p => p.ChangePercent)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            builder.Property(p => p.ImageFileName)
                .HasMaxLength(200);

            builder.Property(p => p.ImageUrl)
                 .HasMaxLength(200);

            builder.Property(p => p.Url)
                .IsRequired()
                .HasMaxLength(1000);
        }
    }
}
