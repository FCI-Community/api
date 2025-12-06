using Graduation_project.Data;
using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using GraduationProject.Models;
using GraduationProject.Statics;
using Microsoft.EntityFrameworkCore;

namespace Graduation_project.Repositories.Implementation
{
    public class MajorRepository : IMajorRepository
    {
        private readonly AppDbContext _db;
        private const string DefaultMajorCode = "general";
        private const string DefaultMajorName = "General";

        public MajorRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<MajorDto>> GetAllMajorsAsync()
        {
            return await _db.Majors
                .AsNoTracking()
                .Select(m => new MajorDto
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    StudentCount = m.StudentProfiles.Count
                })
                .ToListAsync();
        }
        public async Task<MajorDto?> GetMajorByIdAsync(int id)
        {
            var major = await _db.Majors
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(m => new MajorDto
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    StudentCount = m.StudentProfiles.Count
                }).FirstOrDefaultAsync();

            return major;
        }
        public async Task<(bool Success, MajorDto? Major, IEnumerable<string> Errors)> CreateMajorAsync(MajorCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                return (false, null, new[] { "Code and Name are required." });

            var exists = await _db.Majors.AnyAsync(m => m.Code.ToLower() == dto.Code.Trim().ToLower() || m.Name.ToLower() == dto.Name.Trim().ToLower());
            if (exists)
                return (false, null, new[] { "Major with same code or name already exists." });

            var major = new Major
            {
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim()
            };

            _db.Majors.Add(major);
            await _db.SaveChangesAsync();

            var result = new MajorDto { Id = major.Id, Code = major.Code, Name = major.Name, StudentCount = 0 };
            return (true, result, new[] { "Major created successfully." });

        }
        public async Task<(bool Success, MajorDto? Major, IEnumerable<string> Errors)> UpdateMajorAsync(int id, MajorUpdateDto dto)
        {
            var major = await _db.Majors.FindAsync(id);
            if (major == null) return (false, null, new[] { "Major not found." });

            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                return (false, null, new[] { "Code and Name are required." });

            var dup = await _db.Majors.AnyAsync(m => m.Id != id && (m.Code.ToLower() == dto.Code.Trim().ToLower() || m.Name.ToLower() == dto.Name.Trim().ToLower()));
            if (dup) return (false, null, new[] { "Another major with same code or name exists." });

            major.Code = dto.Code.Trim();
            major.Name = dto.Name.Trim();

            await _db.SaveChangesAsync();

            var result = new MajorDto { Id = major.Id, Code = major.Code, Name = major.Name, StudentCount = major.StudentProfiles?.Count ?? 0 };
            return (true, result, new[] { "Major updated successfully." });
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> DeleteMajorAsync(int id)
        {
            var major = await _db.Majors
                .Include(m => m.StudentProfiles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (major == null) return (false, new[] { "Major not found." });

            if (major.StudentProfiles != null && major.StudentProfiles.Any())
                return (false, new[] { "Cannot delete major while students are assigned. Reassign or remove students first." });

            _db.Majors.Remove(major);
            await _db.SaveChangesAsync();
            return (true, new[] { "Major deleted successfully." });
        }
        public async Task<(bool Success, IEnumerable<string> Errors)> AssignStudentToMajorAsync(int majorId, string userId)
        {
            var major = await _db.Majors.FindAsync(majorId);
            if (major == null) return (false, new[] { "Major not found." });

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return (false, new[] { "User not found." });

            var sp = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.AppUserId == userId);
            if (sp == null)
            {
                // create profile and assign
                sp = new StudentProfile
                {
                    AppUserId = userId,
                    StudentId = $"{DateTime.UtcNow.Year}{new Random().Next(1, 9999):0000}",
                    JoinYear = DateTime.UtcNow.Year,
                    AcademicLevel = 1,
                    CurrentSemester = SemesterType.First,
                    ExpectedGradYear = DateTime.UtcNow.Year + 4,
                    MajorId = majorId
                };
                _db.StudentProfiles.Add(sp);
            }
            else
            {
                sp.MajorId = majorId;
            }

            await _db.SaveChangesAsync();
            return (true, new[] { "Student assigned to major." });
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> MoveStudentToMajorAsync(int majorId, string userId)
        {
            var major = await _db.Majors.FindAsync(majorId);
            if (major == null) return (false, new[] { "Major not found." });

            var sp = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.AppUserId == userId);
            if (sp == null) return (false, new[] { "Student profile not found." });

            sp.MajorId = majorId;
            await _db.SaveChangesAsync();
            return (true, new[] { "Student moved to new major." });
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> RemoveStudentFromMajorAsync(string userId)
        {
            var defaultMajor = await GetOrCreateDefaultMajorAsync();
            var sp = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.AppUserId == userId);
            if (sp == null) return (false, new[] { "Student profile not found." });

            sp.MajorId = defaultMajor.Id;
            await _db.SaveChangesAsync();
            return (true, new[] { "Student moved to default major." });
        }

        public async Task<Major> GetOrCreateDefaultMajorAsync()
        {
            var defaultMajor = await _db.Majors.FirstOrDefaultAsync(m => m.Code.ToLower() == DefaultMajorCode);
            if (defaultMajor != null) return defaultMajor;

            defaultMajor = new Major { Code = DefaultMajorCode, Name = DefaultMajorName };
            _db.Majors.Add(defaultMajor);
            await _db.SaveChangesAsync();
            return defaultMajor;
        }
    }
}
