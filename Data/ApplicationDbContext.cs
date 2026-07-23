using Microsoft.EntityFrameworkCore;
using Elwala.Models;

namespace Elwala.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AffiliateRequest> AffiliateRequests { get; set; }
        public DbSet<AffiliatePayment> AffiliatePayments { get; set; }
        public DbSet<AffiliateEventLog> AffiliateEventLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AffiliateRequest>()
                .Property(a => a.Slug)
                .HasMaxLength(150);

            modelBuilder.Entity<AffiliateRequest>()
                .HasIndex(a => a.Slug)
                .IsUnique();

            // Idempotency: one row per (affiliate, event, external key).
            // A duplicate webhook for the same external key is rejected by the
            // unique index, so the caller can skip incrementing the counter.
            // Filtered index so NULL ExternalKeys (legacy bodyless calls) are
            // not constrained and always insert.
            modelBuilder.Entity<AffiliateEventLog>()
                .HasIndex(e => new { e.AffiliateRequestId, e.Event, e.ExternalKey })
                .IsUnique()
                .HasFilter("[ExternalKey] IS NOT NULL");
        }
    }
}
