namespace Graduation_project.Services.IService
{
    public interface IImageManagementService
    {
        Task<string> AddImageAsync(IFormFile file, string src);
        void DeleteImageAsync(string src);
    }
}
