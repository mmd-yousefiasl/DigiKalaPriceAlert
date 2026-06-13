using Domain.Models.GeneralInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.GeneralInfo
{
    public class EmailDataConfig : IEntityTypeConfiguration<EmailData>
    {
        public void Configure(EntityTypeBuilder<EmailData> builder)
        {
            builder.ToTable("EmailData");

            builder.HasKey(n => n.Id);
            builder.HasQueryFilter(n => !n.IsDeleted);

            builder.Property(n => n.From).HasMaxLength(200);
            builder.Property(n => n.Password).HasMaxLength(200);
            builder.Property(n => n.EmailDisplayName).HasMaxLength(200);
            builder.Property(n => n.Smtp).HasMaxLength(200);
        }
    }
}
