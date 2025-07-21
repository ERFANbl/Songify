using Domain.DbMpdels;
using EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3;

namespace Infrastructure.Repositories;

public class  SongRepository :  GenericRepository<Song> , ISongRepository
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

    public async Task<ICollection<Song>?> GetUserSongsAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Songs)
            .FirstOrDefaultAsync(u => u.Id == userId);


        return user?.Songs ?? new List<Song>();
    }
}

