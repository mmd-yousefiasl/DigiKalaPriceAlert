using Domain.Models.GeneralInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.GeneralInfo
{
    public class SiteMapConfig : IEntityTypeConfiguration<SiteMap>
    {
        public void Configure(EntityTypeBuilder<SiteMap> builder)
        {
            builder.ToTable("SiteMap");

            builder.HasKey(sm => sm.Id);

            builder.Property(sm => sm.Url).HasMaxLength(500);
        }
    }
}
