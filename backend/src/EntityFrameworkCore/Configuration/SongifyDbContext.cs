using Domain.DbMpdels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EntityFrameworkCore.Configuration
{
    public class SongifyDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<UserLikedSongs> LikedSongs { get; set; }


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

            modelBuilder.Entity<User>()
                .HasMany(a => a.Songs)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<User>()
                .HasMany(a => a.LikedSongs)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<User>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Song>()
                .HasMany(a => a.LikedByUsers)
                .WithOne(b => b.Song)
                .HasForeignKey(b => b.SongId);

            modelBuilder.Entity<Song>()
                .HasKey(x => x.Id);
        }
    }
}
