using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<OperationEvent> OperationEvents { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OperationEvent>(entity =>
            {
                entity.HasKey(e => new { e.OperationId, e.EventId });
            });
        }
    }
}
