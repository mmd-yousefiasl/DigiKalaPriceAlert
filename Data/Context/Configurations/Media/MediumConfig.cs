using Domain.Models.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Media
{
    public class MediumConfig : IEntityTypeConfiguration<Medium>
    {
        public void Configure(EntityTypeBuilder<Medium> builder)
        {
            builder.ToTable("Media");

            builder.HasKey(n => n.Id);

            builder.HasQueryFilter(n => !n.IsDeleted);

            builder.Property(n => n.FileName)
                .HasMaxLength(50)
                .IsRequired(true);
        }
    }
}
