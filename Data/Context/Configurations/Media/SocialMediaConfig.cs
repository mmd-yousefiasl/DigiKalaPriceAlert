using Domain.Models.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Media
{
    public class SocialMediaConfig : IEntityTypeConfiguration<SocialMedia>
    {
        public void Configure(EntityTypeBuilder<SocialMedia> builder)
        {
            builder.ToTable("SocialMedia");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.FileName).HasMaxLength(50);
            builder.Property(n => n.Title).HasMaxLength(100);
            builder.Property(n => n.Url).HasMaxLength(500);
        }
    }
}
