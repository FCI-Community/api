using Graduation_project.DTOs;
using GraduationProject.Models;

namespace Graduation_project.Repositories.Interfaces
{
    public interface IStudentProfileRepository
    {
        Task<PagedResult<StudentProfileDto>> GetAllAsync(int page = 1, int pageSize = 50);
        Task<(bool Success, StudentProfileDto? Profile, IEnumerable<string> Errors)> UpdateAsync(string appUserId, StudentProfileUpdateDto dto);
        Task<(bool Success, IEnumerable<string> Errors)> DeleteAsync(string appUserId);
        Task<StudentProfile?> GetByUserIdAsync(string appUserId);
    }
}
