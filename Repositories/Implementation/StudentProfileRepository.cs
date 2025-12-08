using Graduation_project.Data;
using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using GraduationProject.Models;
using Microsoft.EntityFrameworkCore;

namespace Graduation_project.Repositories.Implementation
{
    public class StudentProfileRepository : IStudentProfileRepository
    {
        private readonly AppDbContext _db;

        public StudentProfileRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<PagedResult<StudentProfileDto>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _db.StudentProfiles
                .AsNoTracking()
                .Include(sp => sp.AppUser)
                .Include(sp => sp.Major)
                .OrderBy(sp => sp.StudentId)
                .AsQueryable();

            var total = await query.LongCountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(sp => new StudentProfileDto
                {
                    AppUserId = sp.AppUserId,
                    StudentId = sp.StudentId,
                    JoinYear = sp.JoinYear,
                    AcademicLevel = sp.AcademicLevel,
                    CurrentSemester = (int)sp.CurrentSemester,
                    ExpectedGradYear = sp.ExpectedGradYear,
                    MajorId = sp.MajorId,
                    MajorName = sp.Major != null ? sp.Major.Name : null,
                    HideJoinYear = sp.HideJoinYear,
                    HideMajor = sp.HideMajor,
                    HideSubjects = sp.HideSubjects
                })
                .ToListAsync();

            return new PagedResult<StudentProfileDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Items = items
            };
        }

        public async Task<StudentProfile?> GetByUserIdAsync(string appUserId)
        {
            return await _db.StudentProfiles
                .AsNoTracking()
                .Include(sp => sp.Major)
                .Include(sp => sp.AppUser)
                .FirstOrDefaultAsync(s => s.AppUserId == appUserId||s.StudentId==appUserId);
        }

        public async Task<(bool Success, StudentProfileDto? Profile, IEnumerable<string> Errors)> UpdateAsync(string appUserId, StudentProfileUpdateDto dto)
        {
            var sp = await _db.StudentProfiles
                .Include(s => s.AppUser)
                .FirstOrDefaultAsync(s => s.AppUserId == appUserId||s.StudentId==appUserId);

            if (sp == null)
                return (false, null, new[] { "Student profile not found." });

            if (!string.IsNullOrWhiteSpace(dto.StudentId) && dto.StudentId != sp.StudentId)
            {
                var exists = await _db.StudentProfiles.AnyAsync(s => s.StudentId == dto.StudentId);
                if (exists) return (false, null, new[] { "StudentId already in use." });
                sp.StudentId = dto.StudentId;
            }

            if (dto.JoinYear.HasValue) sp.JoinYear = dto.JoinYear.Value;
            if (dto.AcademicLevel.HasValue) sp.AcademicLevel = dto.AcademicLevel.Value;
            if (dto.CurrentSemester.HasValue) sp.CurrentSemester = (GraduationProject.Statics.SemesterType)dto.CurrentSemester.Value;
            if (dto.ExpectedGradYear.HasValue) sp.ExpectedGradYear = dto.ExpectedGradYear.Value;

            if (dto.MajorId.HasValue)
            {
                var majorExists = await _db.Majors.AnyAsync(m => m.Id == dto.MajorId.Value);
                if (!majorExists) return (false, null, new[] { "Major not found." });
                sp.MajorId = dto.MajorId.Value;
            }

            if (dto.HideJoinYear.HasValue) sp.HideJoinYear = dto.HideJoinYear.Value;
            if (dto.HideMajor.HasValue) sp.HideMajor = dto.HideMajor.Value;
            if (dto.HideSubjects.HasValue) sp.HideSubjects = dto.HideSubjects.Value;

            await _db.SaveChangesAsync();

            var result = new StudentProfileDto
            {
                AppUserId = sp.AppUserId,
                StudentId = sp.StudentId,
                JoinYear = sp.JoinYear,
                AcademicLevel = sp.AcademicLevel,
                CurrentSemester = (int)sp.CurrentSemester,
                ExpectedGradYear = sp.ExpectedGradYear,
                MajorId = sp.MajorId,
                MajorName = (await _db.Majors.FindAsync(sp.MajorId))?.Name,
                HideJoinYear = sp.HideJoinYear,
                HideMajor = sp.HideMajor,
                HideSubjects = sp.HideSubjects
            };

            return (true, result, new[] { "Student profile updated successfully." });
        }
        public async Task<(bool Success, IEnumerable<string> Errors)> DeleteAsync(string appUserId)
        {
            var sp = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.AppUserId == appUserId||s.StudentId==appUserId);
            if (sp == null) return (false, new[] { "Student profile not found." });

            _db.StudentProfiles.Remove(sp);
            await _db.SaveChangesAsync();
            return (true, new[] { "Student profile deleted." });
        }
    }
}
