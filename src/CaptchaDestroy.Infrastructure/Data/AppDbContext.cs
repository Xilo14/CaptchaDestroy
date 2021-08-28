using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.EFCore.Extensions;
using CaptchaDestroy.Core.ProjectAggregate;
using CaptchaDestroy.Infrastructure.Data.Mediator;
using CaptchaDestroy.SharedKernel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CaptchaDestroy.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        private readonly Publisher _publisher;

        //public AppDbContext(DbContextOptions options) : base(options)
        //{
        //}

        public AppDbContext(DbContextOptions<AppDbContext> options, Publisher publisher)
            : base(options)
        {
            _publisher = publisher;
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Captcha> Captchas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyAllConfigurationsFromCurrentAssembly();

            // alternately this is built-in to EF Core 2.2
            //modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // ignore events if no dispatcher provided
            if (_publisher == null) return result;

            // dispatch events only if save was successful
            var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
                .Select(e => e.Entity)
                .Where(e => e.Events.Any())
                .ToArray();

            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.Events.ToArray();
                entity.Events.Clear();
                foreach (var domainEvent in events)
                {
                    await _publisher.Publish(domainEvent).ConfigureAwait(false);
                }
            }

            return result;
        }

        public override int SaveChanges()
        {
            return SaveChangesAsync().GetAwaiter().GetResult();
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        public override ValueTask DisposeAsync()
        {
            return base.DisposeAsync();
        }
    }
}