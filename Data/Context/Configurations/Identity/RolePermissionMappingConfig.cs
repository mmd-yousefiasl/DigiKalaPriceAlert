using Domain.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Identity
{
    public class RolePermissionMappingConfig : IEntityTypeConfiguration<RolePermissionMapping>
    {
        public void Configure(EntityTypeBuilder<RolePermissionMapping> builder)
        {
            builder.ToTable("RolePermissionMapping");

            builder.HasKey(rpm => new { rpm.RoleId, rpm.PermissionId });

            builder.HasIndex(rpm => rpm.PermissionId);

            builder.HasOne(rpm => rpm.Role)
                .WithMany(r => r.RolePermissionMapping)
                .HasForeignKey(rpm => rpm.RoleId)
                .IsRequired(false);

            builder.HasOne(rpm => rpm.Permission)
                .WithMany(p => p.RolePermissionMapping)
                .HasForeignKey(rpm => rpm.PermissionId)
                .IsRequired(false);
        }
    }
}
