using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EntityFrameworkCore.Configuration
{
    public class SongifyDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        // Constructor that accepts IConfiguration to access the appsettings.json and environment variables
        public SongifyDbContext(DbContextOptions<SongifyDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Check if options have been configured before
            if (!optionsBuilder.IsConfigured)
            {
                // Get the connection string from environment variable or appsettings.json
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                // Use PostgreSQL as the database provider
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
