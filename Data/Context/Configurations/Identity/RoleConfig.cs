using Domain.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Identity
{
    public class RoleConfig : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);

            builder.HasQueryFilter(r => !r.IsDeleted);

            builder.Property(r => r.Title).HasMaxLength(100);

            builder.Property(r => r.UniqueName).HasMaxLength(100);
        }
    }
}
