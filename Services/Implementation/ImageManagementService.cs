using Graduation_project.Services.IService;
using Microsoft.Extensions.FileProviders;

namespace Graduation_project.Services.Implementation
{
    public class ImageManagementService: IImageManagementService
    {
        private readonly IFileProvider _fileProvider;
        private readonly IWebHostEnvironment _env;

        public ImageManagementService(IFileProvider fileProvider, IWebHostEnvironment env)
        {
            _fileProvider = fileProvider;
            _env = env;
        }
        public async Task<string> AddImageAsync(IFormFile file, string src)
        {

            if (file == null || file.Length == 0) return string.Empty;

            // Ensure src is a safe directory name
            src = string.IsNullOrWhiteSpace(src) ? "Common" : src.Trim().Replace("..", "");

            // Use WebRootPath to get an absolute path (works across hosts)
            var imagesDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "Images", src);

            Directory.CreateDirectory(imagesDir); // safe if already exists


            // sanitize incoming file name and generate a unique name to avoid collisions
            var originalName = Path.GetFileName(file.FileName) ?? "file";
            var ext = Path.GetExtension(originalName);
            if (string.IsNullOrWhiteSpace(ext))
                ext = ".bin";

            var safeFileName = $"{Guid.NewGuid():N}{ext}";

            var physicalPath = Path.Combine(imagesDir, safeFileName);

            // Save file
            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return a URL path that the static file middleware will serve
            var urlPath = $"/Images/{src}/{safeFileName}";
            return urlPath;
        }

        public async Task DeleteImageAsync(string src)
        {
            // src expected to be the url returned by AddImageAsync, e.g. "/Images/Students/abcd.jpg"
            if (string.IsNullOrWhiteSpace(src)) return;

            // Normalize and remove leading slash
            var relative = src.Trim().Replace('\\', '/').TrimStart('/');

            // Build physical path using WebRootPath
            var root = _env.WebRootPath ?? "wwwroot";
            var parts = new[] { root }.Concat(relative.Split('/')).ToArray();
            var physical = Path.Combine(parts);

            // If the file exists, delete it
            if (File.Exists(physical))
            {
                // wrap in Task.Run to keep signature async-friendly
                await Task.Run(() => File.Delete(physical));
            }
        }
    }
}
