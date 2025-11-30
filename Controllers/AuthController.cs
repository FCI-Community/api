using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using Graduation_project.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount(string userId)
        {
            var (success, message) = await _authRepo.DeleteAccountAsync(userId);
            return Ok(new { success, message });
        }
    }
}
