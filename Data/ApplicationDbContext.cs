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
    }
}
