using Domain.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Identity
{
    public class PermissionConfig : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(n => n.Id);

            builder.HasQueryFilter(n => !n.IsDeleted);

            builder.Property(p => p.Title).HasMaxLength(100);

            builder.Property(p => p.UniqueName).HasMaxLength(100);
        }
    }
}
