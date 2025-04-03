using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameAssetStorage.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration configuration)
        {
            _bucketName = configuration["AWS:BucketName"]
                ?? throw new ArgumentNullException("AWS:BucketName is missing in appsettings.json");

            _s3Client = new AmazonS3Client(); // ✅ Uses default AWS credentials (IAM role or env vars)
        }

        public async Task<string> UploadFileAsync(string fileName, Stream fileStream)
        {
            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(fileStream, _bucketName, fileName);
            return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
        }
    }
}
