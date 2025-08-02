using Application.DTOs.Song;
using Application.Interfaces;
using Domain.DbMpdels;
using EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class MadeForUserRepository : IMadeForUserRepository
    {
        protected readonly SongifyDbContext _context;
        public MadeForUserRepository(SongifyDbContext context, IConfiguration config)
        {
            _context = context;
        }

        public async Task<ICollection<GetSongsMetaDataDTO>> GetAllRecomendedSongsAsync(List<string> songIDs, int userId)
        {
            var songs = new List<GetSongsMetaDataDTO>();

            var userLikedSongs = await _context.Users
                                               .Include(e => e.LikedSongs)
                                               .Where(u => u.Id == userId)
                                               .FirstOrDefaultAsync();

            foreach (var songID in songIDs)
            {
                var song = await _context.Songs.Where(x => x.ForigenKey == songID).FirstOrDefaultAsync();

                var songDTO = new GetSongsMetaDataDTO
                {
                    Name = song.Name,
                    Artist = song.Artist,
                    TrackDuration = song.TrackDuration,
                    Lyric = song.Lyric,
                    ForigenKey = song.ForigenKey,
                    Genre = song.Genre,
                    ReleaseDate = song.ReleaseDate,
                    Isliked = userLikedSongs.LikedSongs.Any(s => s.SongId == song.Id),
                };

                songs.Add(songDTO);
            }

            return songs;
        }
    }
}
