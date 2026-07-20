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
    }
}
