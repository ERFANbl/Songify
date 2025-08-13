using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Application.DTOs.Song;
using Domain.DbMpdels;
using EntityFrameworkCore.Configuration;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories;

public class SongRepository : GenericRepository<Song>, ISongRepository
{
    IConfiguration _config;
    protected readonly SongifyDbContext _context;
    public SongRepository(SongifyDbContext context, IConfiguration config) : base(context)
    {
        _config = config;
        _context = context;
    }

    public async Task UploadToS3Async(byte[] songData, string fileName)
    {
        var accessKey = _config["S3:AccessKey"];
        var secretKey = _config["S3:SecretKey"];
        var bucket = _config["S3:BucketName"];
        var serviceUrl = _config["S3:Endpoint"];

        var s3Config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
            RegionEndpoint = Amazon.RegionEndpoint.USEast1,
        };

        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        using var client = new AmazonS3Client(credentials, s3Config);

        var putRequest = new PutObjectRequest
        {
            BucketName = bucket,
            Key = fileName,
            InputStream = new MemoryStream(songData),
            ContentType = "audio/mpeg"
        };

        var response = await client.PutObjectAsync(putRequest);
    }

    public async Task<ICollection<GetSongsMetaDataDTO>?> GetUserSongsAsync(int userId)
    {
        var userSongs = await _context.Users
            .Include(e => e.Songs)
            .Include(e => e.LikedSongs)
            .Where(u => u.Id == userId)
            .ToListAsync();

        //TODO : Need explicit mapper 
        var ResultSelector = userSongs.SelectMany(u => u.Songs
            .Select(song => new GetSongsMetaDataDTO
                {
                    Id = song.Id,
                    Name = song.Name,
                    Artist = song.Artist,
                    TrackDuration = song.TrackDuration,
                    Lyric = song.Lyric,
                    Genre = song.Genre,
                    ReleaseDate = song.ReleaseDate,
                    ForigenKey = song.ForigenKey,
                    Isliked = u.LikedSongs.Any(Is => Is.SongId == song.Id)
                }))
            .ToList();



        return ResultSelector ?? new List<GetSongsMetaDataDTO>();
    }
}

