using GraduationProject.Models;

namespace Graduation_project.Services.IService
{
    public interface IGenerateToken
    {
        string GetAndCreateToken(AppUser user);
    }
}
