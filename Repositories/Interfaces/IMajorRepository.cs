using Graduation_project.DTOs;

namespace Graduation_project.Repositories.Interfaces
{
    public interface IMajorRepository
    {
        Task<(bool Success, MajorDto? Major, IEnumerable<string> Errors)> CreateMajorAsync(MajorCreateDto dto);
        Task<(bool Success, MajorDto? Major, IEnumerable<string> Errors)> UpdateMajorAsync(int id, MajorUpdateDto dto);
        Task<IEnumerable<MajorDto>> GetAllMajorsAsync();
        Task<MajorDto?> GetMajorByIdAsync(int id);
        Task<(bool Success, IEnumerable<string> Errors)> DeleteMajorAsync(int id);

        Task<(bool Success, IEnumerable<string> Errors)> AssignStudentToMajorAsync(int majorId, string userId);
        Task<(bool Success, IEnumerable<string> Errors)> MoveStudentToMajorAsync(int majorId, string userId);
        Task<(bool Success, IEnumerable<string> Errors)> RemoveStudentFromMajorAsync(string userId);
    }
}
