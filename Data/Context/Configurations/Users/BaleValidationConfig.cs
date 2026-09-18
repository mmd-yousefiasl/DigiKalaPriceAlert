using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Users
{
    public class BaleValidationConfig : IEntityTypeConfiguration<BaleValidation>
    {
        public void Configure(EntityTypeBuilder<BaleValidation> builder)
        {
            builder.ToTable("BaleValidation");

            builder.HasKey(uv => uv.Id);
            builder.HasQueryFilter(uv => !uv.IsDeleted);

            builder.Property(uv => uv.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(uv => uv.Code)
                .IsUnique();
        }
    }
}
