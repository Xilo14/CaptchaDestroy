using System;
using System.Linq;
using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CaptchaDestroy.Web
{
    public static class SeedData
    {
        public static readonly Account MyAccount = new Account();

        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var dbContext = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>(), null))
            {
                if (dbContext.Accounts.Any())
                {
                    return;   // DB has been seeded
                }

                PopulateTestData(dbContext);
            }
        }
        public static void PopulateTestData(AppDbContext dbContext)
        {
            dbContext.Accounts.Add(MyAccount);

            dbContext.SaveChanges();
        }
    }
}
