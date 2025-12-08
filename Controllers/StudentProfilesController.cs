using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class StudentProfilesController : ControllerBase
    {
        private readonly IStudentProfileRepository _repo;

        public StudentProfilesController(IStudentProfileRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _repo.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        [HttpGet("{userId}")] 
        public async Task<IActionResult> Get(string userId)
        {
            var sp = await _repo.GetByUserIdAsync(userId);
            if (sp == null) return NotFound();

            var dto = new StudentProfileDto
            {
                AppUserId = sp.AppUserId,
                StudentId = sp.StudentId,
                JoinYear = sp.JoinYear,
                AcademicLevel = sp.AcademicLevel,
                CurrentSemester = (int)sp.CurrentSemester,
                ExpectedGradYear = sp.ExpectedGradYear,
                MajorId = sp.MajorId,
                MajorName = sp.Major?.Name,
                HideJoinYear = sp.HideJoinYear,
                HideMajor = sp.HideMajor,
                HideSubjects = sp.HideSubjects
            };

            return Ok(dto);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> Update(string userId, [FromBody] StudentProfileUpdateDto dto)
        {  
            var (success, profile, message) = await _repo.UpdateAsync(userId, dto);
            if (!success) return BadRequest(new { success = false, message });

            return Ok(new { success = true, profile });
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> Delete(string userId)
        {
            var (success, message) = await _repo.DeleteAsync(userId);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message });
        }
    }
}
