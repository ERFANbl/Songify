using Domain.DbMpdels;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Configuration.ModelsCreatingConfigHandle
{
    public static class ConfigureUserLikedSongs
    {
        /// <summary>
        /// Configures the UserLikedSongs entity in the model builder.
        /// </summary>
        /// <param name="modelBuilder">The model builder to configure.</param>
        /// <remarks>
        /// This method sets up the UserLikedSongs entity with its properties, relationships,
        /// and table mapping. It defines the primary key, foreign keys, and delete behaviors.
        /// </remarks>
        public static void Apply(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserLikedSongs>(entity =>
            {
                entity.ToTable("SNGF_LikedSongs");

                entity.HasKey(uls => uls.Id);

                entity.HasOne(uls => uls.User)
                      .WithMany(u => u.LikedSongs)
                      .HasForeignKey(uls => uls.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(uls => uls.Song)
                      .WithMany(s => s.LikedByUsers)
                      .HasForeignKey(uls => uls.SongId)
                      .OnDelete(DeleteBehavior.NoAction);

            });
        }
    }

}
