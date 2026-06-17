using Domain.Models.ContactUs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.ContactUs
{
    public class ContactUsFormConfig : IEntityTypeConfiguration<ContactUsFrom>
    {
        public void Configure(EntityTypeBuilder<ContactUsFrom> builder)
        {
            builder.ToTable("ContactUsForms");

            builder.HasQueryFilter(cuf => !cuf.IsDeleted);

            builder.HasKey(cuf => cuf.Id);

            builder.Property(cuf => cuf.FullName)
                .HasMaxLength(100);

            builder.Property(cuf => cuf.Mobile)
             .HasMaxLength(100);

            builder.Property(cuf => cuf.Message)
             .HasMaxLength(500);

            builder.Property(cuf => cuf.UserIP)
                .HasMaxLength(20);
        }
    }
}
