using Domain.Models.GeneralInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.GeneralInfo
{
    public class PaymentDataConfig : IEntityTypeConfiguration<PaymentData>
    {
        public void Configure(EntityTypeBuilder<PaymentData> builder)
        {
            builder.ToTable("PaymentData");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.PaymentGatewayPin).HasMaxLength(500);
            builder.Property(n => n.Url).HasMaxLength(1000);
            builder.Property(n => n.FileName).HasMaxLength(50);
        }
    }
}
