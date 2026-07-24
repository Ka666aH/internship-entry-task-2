using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Operation> Operations { get; set; }
    }
}
