using Graduation_project.Data;
using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using Graduation_project.Services.Implementation;
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
        private readonly IGenerateToken _tokenService;

        public AuthRepository(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db,
            IImageManagementService imageService,
            IGenerateToken tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _imageService = imageService;
            _tokenService = tokenService;
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

            if (await _userManager.FindByEmailAsync(adminCreateDto.Email) != null)
            {
                return (false, new[] { "Email is already in use" });
            }

            var user = new AppUser()
            {
                UserName = adminCreateDto.Username,
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

            if (await _userManager.FindByEmailAsync(teacherCreateDto.Email) != null)
            {
                return (false, new[] { "Email is already in use" });
            }

            var user = new AppUser
            {
                UserName =teacherCreateDto.Username,
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

            if (await _userManager.FindByEmailAsync(studentCreateDto.Email) != null)
            {
                return (false, string.Empty, new[] { "Email is already in use" });
            }

            var user = new AppUser
            {
                UserName = studentCreateDto.Username,
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

            int majorIdToAssign;
            if (studentCreateDto.MajorId.HasValue && studentCreateDto.MajorId.Value > 0)
            {
                // ensure major exists
                var majExists = await _db.Majors.AnyAsync(m => m.Id == studentCreateDto.MajorId.Value);
                majorIdToAssign = majExists ? studentCreateDto.MajorId.Value : -1;
            }
            else
            {
                majorIdToAssign = -1;
            }

            if (majorIdToAssign == -1)
            {
                // get or create default General major
                var defaultMajor = await _db.Majors.FirstOrDefaultAsync(m => m.Code.ToLower() == "general");
                if (defaultMajor == null)
                {
                    defaultMajor = new Major { Code = "general", Name = "General" };
                    _db.Majors.Add(defaultMajor);
                    await _db.SaveChangesAsync();
                }
                majorIdToAssign = defaultMajor.Id;
            }

            var studentProfile = new StudentProfile
            {
                AppUserId = user.Id,
                StudentId = studentId,
                JoinYear = studentCreateDto.JoinYear,
                AcademicLevel = 1,
                CurrentSemester = SemesterType.First,
                ExpectedGradYear = studentCreateDto.JoinYear + 4,
                MajorId = majorIdToAssign
            };

            _db.StudentProfiles.Add(studentProfile);
            await _db.SaveChangesAsync();

            return (true, studentId, new List<string> { "Student created successfully" });

        }

        public async Task<(bool Success, IEnumerable<string> Errors)> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, new[] { "User not found" });

            if (!string.IsNullOrWhiteSpace(user.ProfilePictureUrl))
            {
                try
                {
                    await _imageService.DeleteImageAsync(user.ProfilePictureUrl);
                }
                catch (Exception)
                {
                }
            }

            var delete = await _userManager.DeleteAsync(user);
            if (!delete.Succeeded) return (false, delete.Errors.Select(e => e.Description));

            return (true, new List<string> { "Account deleted successfully" });
        }

        public async Task<(bool Success, string Token, AppUser? User, IEnumerable<string> Errors)> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return (false, string.Empty, null, new[] { "Invalid credentials" });

            // verify password
            var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordValid)
                return (false, string.Empty, null, new[] { "Invalid credentials" });

            // generate JWT token
            var token = _tokenService.GetAndCreateToken(user);

            return (true, token, user, new List<string> { "Login successful" });
        }

        public async Task<PagedResult<UserListItemDto>> GetUsersAsync(UserListQueryDto query)
        {
            var users = _db.Users
                  .AsNoTracking()
                  .Include(u => u.StudentProfile)
                  .Include(u => u.SubjectsStaff)
                  .AsQueryable();

            // Role filter
            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                var roleLower = query.Role.Trim().ToLowerInvariant();
                switch (roleLower)
                {
                    case "admin":
                        users = users.Where(u => u.staffRole == StaffRole.Admin);
                        break;
                    case "prof":
                    case "professor":
                        users = users.Where(u => u.staffRole == StaffRole.Professor);
                        break;
                    case "assist":
                    case "assistant":
                        users = users.Where(u => u.staffRole == StaffRole.Assistant);
                        break;
                    case "student":
                        users = users.Where(u => u.staffRole == StaffRole.Student);
                        break;
                    default:
                        break;
                }
            }

            // Major filter (students only)
            if (query.MajorId.HasValue)
            {
                var mid = query.MajorId.Value;
                if (mid == -1)
                {
                    users = users.Where(u => u.StudentProfile == null);
                }
                else if (mid > 0)
                {
                    users = users.Where(u => u.StudentProfile != null && u.StudentProfile.MajorId == mid);
                }
            }

            // Join year (students only)
            if (query.JoinYear.HasValue)
            {
                var jy = query.JoinYear.Value;
                users = users.Where(u => u.StudentProfile != null && u.StudentProfile.JoinYear == jy);
            }

            // Subject filter (matches SubjectStaff entries)
            if (query.SubjectId.HasValue)
            {
                var sid = query.SubjectId.Value;
                users = users.Where(u => u.SubjectsStaff.Any(ss => ss.SubjectId == sid));
            }

            // Search: numeric => student id, otherwise name/email
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim();
                if (s.All(char.IsDigit))
                {
                    users = users.Where(u => u.StudentProfile != null && u.StudentProfile.StudentId.Contains(s));
                }
                else
                {
                    var lowered = s.ToLower();
                    users = users.Where(u => u.FullName.ToLower().Contains(lowered) || (u.Email != null && u.Email.ToLower().Contains(lowered)));
                }
            }

            // total
            var total = await users.LongCountAsync();

            // pagination
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, 200);

            var items = await users
               .OrderBy(u => u.FullName)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .Select(u => new UserListItemDto
               {
                  Id = u.Id,
                  FullName = u.FullName,
                  UserName = u.UserName,
                  Email = u.Email,
                  ProfilePictureUrl = u.ProfilePictureUrl,
                  Role = u.staffRole == StaffRole.Admin ? "admin"
                   : u.staffRole == StaffRole.Professor ? "prof"
                   : u.staffRole == StaffRole.Assistant ? "assist"
                   : "student",
                 StudentId = u.StudentProfile == null ? null : u.StudentProfile.StudentId,
                 MajorId = u.StudentProfile == null ? null : (int?)u.StudentProfile.MajorId,
                 JoinYear = u.StudentProfile == null ? null : (int?)u.StudentProfile.JoinYear
               })
               .ToListAsync();

            return new PagedResult<UserListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Items = items
            };
        }

        public async Task<AppUser?> GetUserProfileAsync(string userId)
        {
            return await _db.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
        }
    }
}
