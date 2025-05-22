using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Linkitty.Models
{
    public class UrlDbContext : DbContext
    {
        public DbSet<UrlMapping> UrlMappings { get; set; }

        public UrlDbContext(DbContextOptions<UrlDbContext> options)
            : base(options) {}
    }
}
