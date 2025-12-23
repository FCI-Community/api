
using Graduation_project.DTOs;
using GraduationProject.Statics;

namespace Graduation_project.Repositories.Interfaces
{
    public interface ISubjectRepository
    {
        Task<(bool Success, SubjectDto? Subject, IEnumerable<string> Errors)> CreateSubjectAsync(SubjectCreateDto dto);
        Task<IEnumerable<SubjectDto>> GetAllSubjectsAsync();
        Task<SubjectDto?> GetSubjectByIdAsync(int id);
        Task<(bool Success, IEnumerable<string> Errors)> DeleteSubjectAsync(int id);
        Task<(bool Success, IEnumerable<string> Errors)> AssignStudentToSubjectAsync(int subjectId, string userId);
        Task<(bool Success, IEnumerable<string> Errors)> RemoveStudentFromSubjectAsync(string userId);
        Task<(bool Success, IEnumerable<string> Errors)> AssignSubjectStaffToSubjectAsync(int subjectId, int subjectStaffId, StaffRole staffRole);
        Task<(bool Success, IEnumerable<string> Errors)> RemoveSubjectStaffFromSubjectAsync(int subjectStaffId);

    }
}
