using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Users
{
    public class UserProductMappingConfig : IEntityTypeConfiguration<UserProductMapping>
    {
        public void Configure(EntityTypeBuilder<UserProductMapping> builder)
        {
            builder.ToTable("UserProductMappings");

            builder.HasKey(up => up.Id);

            builder.HasQueryFilter(up => !up.IsDeleted);

            builder.Property(up => up.IsRead)
                .HasDefaultValue(false);

            builder.HasIndex(up => new { up.UserId, up.VariantSellerId })
                .IsUnique();

            builder.HasIndex(up => up.UserId);
            builder.HasIndex(up => up.VariantSellerId);

            builder.HasOne(up => up.User)
                .WithMany(u => u.Products)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(up => up.VariantSeller)
                .WithMany(p => p.Users)
                .HasForeignKey(up => up.VariantSellerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}