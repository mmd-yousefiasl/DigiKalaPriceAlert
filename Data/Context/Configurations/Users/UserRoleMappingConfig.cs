using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Users
{
    public class UserRoleMappingConfig : IEntityTypeConfiguration<UserRoleMapping>
    {
        public void Configure(EntityTypeBuilder<UserRoleMapping> builder)
        {
            builder.ToTable("UserRoleMapping");

            builder.HasKey(urm => new { urm.UserId, urm.RoleId });

            builder.HasIndex(urm => urm.RoleId);

            builder.HasOne(urm => urm.User)
                .WithMany(u => u.UserRoleMapping)
                .HasForeignKey(urm => urm.UserId)
                .IsRequired(false);

            builder.HasOne(urm => urm.Role)
                .WithMany(u => u.UserRoleMapping)
                .HasForeignKey(urm => urm.RoleId)
                .IsRequired(false);
        }
    }
}
