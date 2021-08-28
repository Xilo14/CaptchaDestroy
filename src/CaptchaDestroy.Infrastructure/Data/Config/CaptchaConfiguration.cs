using CaptchaDestroy.Core.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CaptchaDestroy.Infrastructure.Data.Config
{
    public class CaptchaConfiguration : IEntityTypeConfiguration<Captcha>
    {
        public void Configure(EntityTypeBuilder<Captcha> builder)
        {
            // builder.Property(t => t.)
            //     .IsRequired();
        }
    }
}
