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

            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.Property(p => p.DigiKalaId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.ImageUrl)
                 .HasMaxLength(1000);

            builder.Property(p => p.Url)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasIndex(p => p.DigiKalaId)
                .IsUnique();
        }
    }
}
