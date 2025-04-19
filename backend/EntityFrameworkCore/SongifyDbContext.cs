using Microsoft.EntityFrameworkCore;
using Songify.Domain.Entities;

namespace Songify.Infrastructure.Data
{
    public class SongifyDbContext : DbContext
    {
        public SongifyDbContext(DbContextOptions<SongifyDbContext> options)
            : base(options)
        {
        }

        // Add your DbSet properties here for each entity
        // Example:
        // public DbSet<Song> Songs { get; set; }
        // public DbSet<Artist> Artists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Add your entity configurations here
            // Example:
            // modelBuilder.ApplyConfiguration(new SongConfiguration());
        }
    }
} 