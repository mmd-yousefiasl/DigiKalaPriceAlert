using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Users
{
    public class UserProductMappingConfig : IEntityTypeConfiguration<UserProductMapping>
    {
        public void Configure(EntityTypeBuilder<UserProductMapping> builder)
        {
            builder.ToTable("UserProductMapping");

            builder.HasKey(upm => new { upm.UserId, upm.ProductId });

            builder.HasIndex(upm => new { upm.UserId, upm.ProductId })
                .IncludeProperties(upm => upm.IsRead);

            builder.HasIndex(upm => upm.ProductId);

            builder.HasOne(upm => upm.User)
                .WithMany(u => u.Products)
                .HasForeignKey(upm => upm.UserId)
                .IsRequired(false);

            builder.HasOne(upm => upm.Product)
                .WithMany(p => p.Users)
                .HasForeignKey(upm => upm.ProductId);
        }
    }
}
