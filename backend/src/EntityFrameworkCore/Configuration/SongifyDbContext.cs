using Domain.DbMpdels;
using EntityFrameworkCore.Configuration.ModelsCreatingConfigHandle;
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

            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

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

            modelBuilder.Entity<Song>()
                .HasKey(x => x.Id);

            // Configure the UserLikedSongs entity
            ConfigureUserLikedSongs.Apply(modelBuilder);
        }
    }
}
