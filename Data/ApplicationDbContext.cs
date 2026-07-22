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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AffiliateRequest>()
                .Property(a => a.Slug)
                .HasMaxLength(150);

            modelBuilder.Entity<AffiliateRequest>()
                .HasIndex(a => a.Slug)
                .IsUnique();
        }
    }
}
