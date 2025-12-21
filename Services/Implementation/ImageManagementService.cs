using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DotNetEnv;
using Graduation_project.Services.IService;
using Microsoft.Extensions.FileProviders;

namespace Graduation_project.Services.Implementation
{
    public class ImageManagementService: IImageManagementService
    {
        private readonly BlobContainerClient _container;
        public ImageManagementService()
        {
            var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            var containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME");

            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(containerName))
                throw new Exception("Azure Blob Storage environment variables are missing");

            _container = new BlobContainerClient(connectionString, containerName);
            _container.CreateIfNotExists(PublicAccessType.Blob);
        }


        public async Task<string> AddImageAsync(IFormFile file, string src)
        {

            if (file == null || file.Length == 0)
                return string.Empty;

            src = string.IsNullOrWhiteSpace(src) ? "Common" : src.Trim();

            var extension = Path.GetExtension(file.FileName);
            var blobName = $"{src}/{Guid.NewGuid():N}{extension}";

            var blobClient = _container.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = file.ContentType
            });

            return blobClient.Uri.ToString();
        }

        public async Task DeleteImageAsync(string src)
        {
            if (string.IsNullOrWhiteSpace(src))
                return;

            var uri = new Uri(src);

            // remove "/container-name/"
            var blobName = uri.AbsolutePath
                .Replace($"/{_container.Name}/", "");

            await _container.DeleteBlobIfExistsAsync(blobName);
        }
    }
}
