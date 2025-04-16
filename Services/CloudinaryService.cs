using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace GameAssetStorage.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            await using var stream = file.OpenReadStream();

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "gameassets",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = true // optional
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString() ?? string.Empty;
        }


        public async Task<string> UploadAudioAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            await using var stream = file.OpenReadStream();

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "gameassets",
                UseFilename = true,
                UniqueFilename = true
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString() ?? string.Empty;
        }



        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var parts = uri.AbsolutePath.Split('/');
                var filenameWithExt = parts.Last();
                var filename = filenameWithExt.Split('.').First();
                var folder = parts[^2];
                var publicId = $"{folder}/{filename}";

                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                return result.Result == "ok" || result.Result == "not found";
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Cloudinary delete error: " + ex.Message);
                return false;
            }
        }
    }
}
