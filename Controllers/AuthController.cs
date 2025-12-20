using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using Graduation_project.Services.IService;
using GraduationProject.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuth _authRepo;

        public AuthController(IAuth authRepo)
        {
            _authRepo = authRepo;
        }

        [HttpPost("teacher-register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> TeacherRegister([FromForm] TeacherCreateDto teacherCreateDto)
        {
            var (success, message) = await _authRepo.CreateTeacherAsync(teacherCreateDto);
            return Ok(new { success, message });
        }

        [HttpPost("admin-register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AdminRegister([FromForm] AdminCreateDto adminCreateDto)
        {
            var (success, message) = await _authRepo.CreateAdminAsync(adminCreateDto);
            return Ok(new { success, message });
        }

        [HttpPost("student-register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> StudentRegister([FromForm] StudentCreateDto studentCreateDto)
        {
            var (success, studentId, message) = await _authRepo.CreateStudentAsync(studentCreateDto);
            return Ok(new { success, studentId, message });
        }

        [HttpDelete("delete-account/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount(string userId)
        {
            var (success, message) = await _authRepo.DeleteAccountAsync(userId);
            return Ok(new { success, message });
        }

        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin([FromBody] LoginDto loginDto)
        {
            var (success, token, user, message) = await _authRepo.LoginAsync(loginDto);

            if (!success)
            {
                return Unauthorized(new { success, message });
            }

            if (user == null || user.staffRole != StaffRole.Admin)
            {
                return NotFound();
            }

            var response = new LoginResponseDto
            {
                Token = token,
                Role = "admin",
                Profile = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    Role = user.staffRole.ToString()
                }
            };

            return Ok(response);
        }

        [HttpPost("user-login")]
        public async Task<IActionResult> UserLogin([FromBody] LoginDto loginDto)
        {
            var (success, token, user, message) = await _authRepo.LoginAsync(loginDto);

            if (!success)
            {
                return Unauthorized(new { success, message });
            }

            if (user == null)
            {
                return BadRequest(new { success=false, message= new[] { "User not found " } });
            }

            if (user.staffRole == StaffRole.Admin)
            {
                return NotFound();
            }

            string roleShort = user.staffRole switch
            {
                StaffRole.Professor => "prof",
                StaffRole.Assistant => "assist",
                StaffRole.Student => "student",
                _ => "student"
            };

            object profile = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.ProfilePictureUrl,
                Role = user.staffRole.ToString(),
                StudentProfile = user.StudentProfile == null ? null : new
                {
                    user.StudentProfile.StudentId,
                    user.StudentProfile.JoinYear,
                    user.StudentProfile.AcademicLevel
                }
            };

            var response = new LoginResponseDto
            {
                Token = token,
                Role = roleShort,
                Profile = profile
            };

            return Ok(response);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserListQueryDto query)
        {
            var result = await _authRepo.GetUsersAsync(query);
            return Ok(result);
        }

        [HttpGet("user-profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _authRepo.GetUserProfileAsync(userId);
            if (user == null) return NotFound();

            var dto = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName,
                Email = user.Email,
                Role = user.staffRole.ToString(),
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return Ok(dto);
        }

        [HttpGet("user-ById/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _authRepo.GetUserProfileAsync(userId);
            if (user == null) return NotFound();

            var dto = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName,
                Email = user.Email,
                Role = user.staffRole.ToString(),
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return Ok(dto);
        }
    }
}
