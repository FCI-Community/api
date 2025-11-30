using Graduation_project.Data;
using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using Graduation_project.Services.IService;
using GraduationProject.Models;
using GraduationProject.Statics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Graduation_project.Repositories.Implementation
{
    public class AuthRepository : IAuth
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;
        private readonly IImageManagementService _imageService;

        public AuthRepository(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db,
            IImageManagementService imageService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _imageService = imageService;
        }

        private async Task EnsureRolesAsync()
        {
            var roles = new[] { nameof(StaffRole.Admin), nameof(StaffRole.Professor), nameof(StaffRole.Assistant), nameof(StaffRole.Student) };
            foreach (var r in roles)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole(r));
            }
        }
        public async Task<(bool Success, IEnumerable<string> Errors)> CreateAdminAsync(AdminCreateDto adminCreateDto)
        {
            await EnsureRolesAsync();

            var user = new AppUser()
            {
                UserName = adminCreateDto.Email,
                FullName = adminCreateDto.FullName,
                Email = adminCreateDto.Email,
                staffRole = StaffRole.Admin
            };

            var result = await _userManager.CreateAsync(user, adminCreateDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, nameof(StaffRole.Admin));
                return (true, new List<string> { "Admin created successfully" });
            }
            else
            {
                return (false, result.Errors.Select(e => e.Description));
            }
        }
        public async Task<(bool Success, IEnumerable<string> Errors)> CreateTeacherAsync(TeacherCreateDto teacherCreateDto)
        {
             await EnsureRolesAsync();

            var user = new AppUser
            {
                UserName =teacherCreateDto.FullName,
                Email =teacherCreateDto.Email,
                FullName =teacherCreateDto.FullName,
                staffRole =teacherCreateDto.Role?.ToLower() switch
                {
                    "prof" or "professor" => StaffRole.Professor,
                    "assist" or "assistant" => StaffRole.Assistant,
                    _ => StaffRole.Professor
                }
            };

            if (teacherCreateDto.CoverImage != null)
            {
                var imageUrl = await _imageService.AddImageAsync(teacherCreateDto.CoverImage, "Teachers");
                user.ProfilePictureUrl = imageUrl;
            }

            var result = await _userManager.CreateAsync(user, teacherCreateDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, user.staffRole.ToString());
                return (true, new List<string> { "Teacher created successfully" });
            }
            else
            {
                return (false, result.Errors.Select(e => e.Description));
            }
        }
        public async Task<(bool Success, string StudentId, IEnumerable<string> Errors)> CreateStudentAsync(StudentCreateDto studentCreateDto)
        {
            await EnsureRolesAsync();

            var user = new AppUser
            {
                UserName = studentCreateDto.FullName,
                Email = studentCreateDto.Email,
                FullName = studentCreateDto.FullName,
                staffRole = StaffRole.Student,
            };

            if (studentCreateDto.CoverImage != null)
            {
                var imageUrl = await _imageService.AddImageAsync(studentCreateDto.CoverImage, "Students");
                user.ProfilePictureUrl = imageUrl;
            }

            var result = await _userManager.CreateAsync(user, studentCreateDto.Password);

            if (!result.Succeeded)
            {
               return (false, string.Empty, result.Errors.Select(e => e.Description));
            }

            await _userManager.AddToRoleAsync(user, user.staffRole.ToString());

            var existing = await _db.StudentProfiles
                .Where(sp => sp.JoinYear == studentCreateDto.JoinYear)
                .OrderByDescending(sp => sp.StudentId)
                .FirstOrDefaultAsync();

            int nextSerial = 1;
            if (existing != null && !string.IsNullOrWhiteSpace(existing.StudentId) && existing.StudentId.Length >= 4)
            {
                var last4 = existing.StudentId[^4..];
                if (int.TryParse(last4, out var parsed))
                    nextSerial = parsed + 1;
            }

            var studentId = $"{studentCreateDto.JoinYear}{nextSerial:0000}";

            var studentProfile = new StudentProfile
            {
                AppUserId = user.Id,
                StudentId = studentId,
                JoinYear = studentCreateDto.JoinYear,
                AcademicLevel = 1,
                CurrentSemester = SemesterType.First,
                ExpectedGradYear = studentCreateDto.JoinYear + 4
            };

            _db.StudentProfiles.Add(studentProfile);
            await _db.SaveChangesAsync();

            return (true, studentId, new List<string> { "Student created successfully" });

        }

        public async Task<(bool Success, IEnumerable<string> Errors)> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, new[] { "User not found" });

            _imageService.DeleteImageAsync(user.ProfilePictureUrl);

            var delete = await _userManager.DeleteAsync(user);
            if (!delete.Succeeded) return (false, delete.Errors.Select(e => e.Description));

            return (true, new List<string> { "Account deleted successfully" });
        }
    }
}
