using Graduation_project.DTOs;
using GraduationProject.Models;

namespace Graduation_project.Repositories.Interfaces
{
    public interface IAuth
    {
        Task<(bool Success, IEnumerable<string> Errors)> CreateTeacherAsync(TeacherCreateDto teacherCreateDto);
        Task<(bool Success, IEnumerable<string> Errors)> CreateAdminAsync(AdminCreateDto adminCreateDto);
        Task<(bool Success, string StudentId, IEnumerable<string> Errors)> CreateStudentAsync(StudentCreateDto studentCreateDto);
        Task<(bool Success, IEnumerable<string> Errors)> DeleteAccountAsync(string userId);
        Task<(bool Success, string Token, AppUser? User, IEnumerable<string> Errors)> LoginAsync(LoginDto loginDto);
        Task<PagedResult<UserListItemDto>> GetUsersAsync(UserListQueryDto query);

    }
}
