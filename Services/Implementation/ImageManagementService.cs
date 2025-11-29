using Graduation_project.Services.IService;
using Microsoft.Extensions.FileProviders;

namespace Graduation_project.Services.Implementation
{
    public class ImageManagementService: IImageManagementService
    {
        private readonly IFileProvider fileProvider;

        public ImageManagementService(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider;
        }
        public async Task<string> AddImageAsync(IFormFile file, string src)
        {

            var ImageDirectory = Path.Combine("wwwroot", "Images", src);

            if (!Directory.Exists(ImageDirectory))
            {
                Directory.CreateDirectory(ImageDirectory);
            }


            if (file.Length > 0)
            {
                var ImageName = file.FileName;
                var ImageSrc = $"/Images/{src}/{ImageName}";
                var root = Path.Combine(ImageDirectory, ImageName);

                using (FileStream stream = new FileStream(root, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return ImageSrc;
            }

            return "";
        }

        public void DeleteImageAsync(string src)
        {
            var info = fileProvider.GetFileInfo(src);

            var root = info.PhysicalPath;
            File.Delete(root);
        }
    }
}
