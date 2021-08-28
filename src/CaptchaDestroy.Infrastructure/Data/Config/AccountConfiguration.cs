using CaptchaDestroy.Core.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaptchaDestroy.Infrastructure.Data.Config
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.Property(a => a.SecretKey)
                .HasMaxLength(16)
                .IsRequired();

            builder.HasIndex(a => a.SecretKey)
                .IsUnique();
            // builder.Property(p => p.)
            //     .HasMaxLength(100)
            //     .IsRequired();
        }
    }
}
