using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;

namespace GameAssetStorage.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string? _bucketName;

        public S3Service(IConfiguration configuration)
        {
            var awsAccessKey = configuration["AWS:AccessKeyId"] ?? throw new ArgumentNullException("AWS:AccessKeyId");
            var awsSecretKey = configuration["AWS:SecretAccessKey"] ?? throw new ArgumentNullException("AWS:SecretAccessKey");
            _bucketName = configuration["AWS:BucketName"] ?? throw new ArgumentNullException("AWS:BucketName");

            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.USEast1
            };

            _s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, config);
        }

        // Uploads the file to S3 and returns the public URL
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = fileName,
                BucketName = _bucketName,
                ContentType = file.ContentType
                // ✅ Removed: CannedACL = S3CannedACL.PublicRead
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
        }

        // Deletes the file from S3 using its full public URL
        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            var key = fileUrl.Replace($"https://{_bucketName}.s3.amazonaws.com/", "");

            await _s3Client.DeleteObjectAsync(_bucketName, key);
        }

        // ✅ Streams file from S3 for download
        public async Task<Stream> GetFileStreamAsync(string fileUrl)
        {
            var key = fileUrl.Replace($"https://{_bucketName}.s3.amazonaws.com/", "");
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
    }
}
