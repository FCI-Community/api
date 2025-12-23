using Graduation_project.Data;
using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using GraduationProject.Models;
using GraduationProject.Statics;
using Microsoft.EntityFrameworkCore;

namespace Graduation_project.Repositories.Implementation
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly AppDbContext _db;
        private const string DefaultSubjectCode = "GN000";
        private const string DefaultSubjectName = "General";

        public SubjectRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<SubjectDto>> GetAllSubjectsAsync()
        {
            return await _db.Subjects
                .AsNoTracking()
                .Select(m => new SubjectDto
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    year = m.year,
                    Semester = m.Semester,
                    StudentProfiles = m.StudentProfiles,
                    SubjectStaff = m.SubjectStaff
                })
                .ToListAsync();
        }
        public async Task<SubjectDto?> GetSubjectByIdAsync(int id)
        {
            var Subject = await _db.Subjects
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(m => new SubjectDto
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    year = m.year,
                    Semester = m.Semester,
                    StudentProfiles = m.StudentProfiles,
                    SubjectStaff = m.SubjectStaff
                }).FirstOrDefaultAsync();

            return Subject;
        }
        public async Task<(bool Success, SubjectDto? Subject, IEnumerable<string> Errors)> CreateSubjectAsync(SubjectCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                return (false, null, new[] { "Code and Name are required." });

            var exists = await _db.Subjects.AnyAsync(m => m.Code.ToLower() == dto.Code.Trim().ToLower() || m.Name.ToLower() == dto.Name.Trim().ToLower());
            if (exists)
                return (false, null, new[] { "Subject with same code or name already exists." });

            var Subject = new Subject
            {
                Name = dto.Name,
                Code = dto.Code,
                year = dto.year,
                Semester = dto.Semester == 0 ? SemesterType.First : dto.Semester == 1 ? SemesterType.Second : SemesterType.Summer
            };

            _db.Subjects.Add(Subject);
            await _db.SaveChangesAsync();

            var result = new SubjectDto
            {
                Id = Subject.Id,
                Code = Subject.Code,
                Name = Subject.Name,
                year = Subject.year,
                Semester = Subject.Semester,
                StudentProfiles = Subject.StudentProfiles,
                SubjectStaff = Subject.SubjectStaff
            };
            return (true, result, new[] { "Subject created successfully." });

        }
        public async Task<(bool Success, IEnumerable<string> Errors)> DeleteSubjectAsync(int id)
        {
            var Subject = await _db.Subjects
                .Include(m => m.StudentProfiles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Subject == null) return (false, new[] { "Subject not found." });

            if (Subject.StudentProfiles != null && Subject.StudentProfiles.Any())
                return (false, new[] { "Cannot delete Subject while students are assigned. Reassign or remove students first." });

            _db.Subjects.Remove(Subject);
            await _db.SaveChangesAsync();
            return (true, new[] { "Subject deleted successfully." });
        }
        public async Task<(bool Success, IEnumerable<string> Errors)> AssignStudentToSubjectAsync(int SubjectId, string userId)
        {
            var Subject = await _db.Subjects.FindAsync(SubjectId);
            if (Subject == null) return (false, new[] { "Subject not found." });

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
                    ExpectedGradYear = DateTime.UtcNow.Year + 4
                };
                _db.StudentProfiles.Add(sp);
            }

            await _db.SaveChangesAsync();
            return (true, new[] { "Student assigned to Subject." });
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> RemoveStudentFromSubjectAsync(string userId)
        {
            var defaultSubject = await GetOrCreateDefaultSubjectAsync();
            var sp = await _db.StudentProfiles.FirstOrDefaultAsync(s => s.AppUserId == userId);
            if (sp == null) return (false, new[] { "Student profile not found." });

            await _db.SaveChangesAsync();
            return (true, new[] { "Student moved to default Subject." });
        }

        

        public async Task<(bool Success, IEnumerable<string> Errors)> AssignSubjectStaffToSubjectAsync(int SubjectId, int subjectStaffId, StaffRole staffRole)
        {
            var Subject = await _db.Subjects.FindAsync(SubjectId);
            if (Subject == null) return (false, new[] { "Subject not found." });

            var subjectStaff = await _db.Users.FindAsync(subjectStaffId);
            if (subjectStaff == null) return (false, new[] { "Subject Staff not found." });

            var sp = await _db.SubjectStaffs.FirstOrDefaultAsync(s => s.Id == subjectStaffId);
            if (sp == null)
            {
                // create profile and assign
                sp = new SubjectStaff
                {
                    Id = subjectStaffId,
                    AppUserId = subjectStaff.Id,
                    SubjectId = SubjectId,
                    Role = staffRole
                };
                _db.SubjectStaffs.Add(sp);
            }

            await _db.SaveChangesAsync();
            return (true, new[] { "Subject Staffs assigned to Subject." });
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> RemoveSubjectStaffFromSubjectAsync(int subjectStaffId)
        {
            var defaultSubject = await GetOrCreateDefaultSubjectAsync();
            var sp = await _db.SubjectStaffs.FirstOrDefaultAsync(s => s.Id == subjectStaffId);
            if (sp == null) return (false, new[] { "Subject staff not found." });

            await _db.SaveChangesAsync();
            return (true, new[] { "Subject staff removed successfully." });
            throw new NotImplementedException();
        }
        public async Task<Subject> GetOrCreateDefaultSubjectAsync()
        {
            var defaultSubject = await _db.Subjects.FirstOrDefaultAsync(m => m.Code.ToLower() == DefaultSubjectCode);
            if (defaultSubject != null) return defaultSubject;

            defaultSubject = new Subject { Code = DefaultSubjectCode, Name = DefaultSubjectName };
            _db.Subjects.Add(defaultSubject);
            await _db.SaveChangesAsync();
            return defaultSubject;
        }
    }
}
