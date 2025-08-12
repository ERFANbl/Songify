using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3;
using Application.Interfaces;
using Domain.DbMpdels;
using EntityFrameworkCore.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class StreamRepository : IStreamRepository
    {
        IConfiguration _config;
        public StreamRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<(Stream stream, long start, long end, long total)> GetAudioRangeAsync(string fileName, string rangeHeader)
        {
            var accessKey = _config["S3:AccessKey"];
            var secretKey = _config["S3:SecretKey"];
            var bucket = _config["S3:BucketName"];
            var serviceUrl = _config["S3:Endpoint"];

            var s3Config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = true,
            };

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            using var client = new AmazonS3Client(credentials, s3Config);

            var metadata = await client.GetObjectMetadataAsync(bucket, fileName+".mp3");
            var totalSize = metadata.ContentLength;

            long start = 0;
            long end = totalSize - 1;

            if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes="))
            {
                var range = rangeHeader.Replace("bytes=", "").Split('-');
                start = long.Parse(range[0]);
                if (range.Length > 1 && !string.IsNullOrEmpty(range[1]))
                    end = long.Parse(range[1]);
            }

            var request = new GetObjectRequest
            {
                BucketName = bucket,
                Key = fileName+".mp3",
                ByteRange = new ByteRange(start, end)
            };

            var response = await client.GetObjectAsync(request);
            return (response.ResponseStream, start, end, totalSize);
        }
    }
}
