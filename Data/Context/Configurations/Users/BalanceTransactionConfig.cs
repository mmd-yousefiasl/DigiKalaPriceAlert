using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context.Configurations.Users
{
    public class BalanceTransactionConfig : IEntityTypeConfiguration<BalanceTransaction>
    {
        public void Configure(EntityTypeBuilder<BalanceTransaction> builder)
        {
            builder.ToTable("BalanceTransactions");

            builder.HasKey(bt => bt.Id);
            builder.HasQueryFilter(bt => !bt.IsDeleted);

            builder.Property(bt => bt.Description).HasMaxLength(500);
            builder.Property(bt => bt.TransId).HasMaxLength(200);

            builder.Property(bt => bt.Amount)
                .HasColumnType("numeric(18,0)");

            builder.HasOne(bt => bt.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(bt => bt.UserId);
        }
    }
}
