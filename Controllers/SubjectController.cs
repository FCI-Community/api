using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using GraduationProject.Models;
using GraduationProject.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectRepository _subjects;

        public SubjectsController(ISubjectRepository subjects)
        {
            _subjects = subjects;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _subjects.GetAllSubjectsAsync();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var Subject = await _subjects.GetSubjectByIdAsync(id);
            if (Subject == null) return NotFound();
            return Ok(Subject);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubjectCreateDto dto)
        {
            var (success, subject, message) = await _subjects.CreateSubjectAsync(dto);
            if (!success) return BadRequest(new { success = false, message });
            return CreatedAtAction(nameof(Get), new { id = subject!.Id }, subject);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _subjects.DeleteSubjectAsync(id);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success, message });
        }

        // assign student to Subject
        [HttpPost("{subjectId:int}/assignStudent/{userId}")]
        public async Task<IActionResult> AssignStudent(int subjectId, string userId)
        {
            var (success, message) = await _subjects.AssignStudentToSubjectAsync(subjectId, userId);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message });
        }
        // remove student from Subject -> move to default General
        [HttpPost("removeStudent/{userId}")]
        public async Task<IActionResult> RemoveStudent(string userId)
        {
            var (success, message) = await _subjects.RemoveStudentFromSubjectAsync(userId);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message });
        }
        [HttpPost("{SubjectId:int}/assignStaff/{subjectStaffId}/{staffRole}")]
        public async Task<IActionResult> AssignSubjectStaff(int SubjectId, int subjectStaffId, StaffRole staffRole)
        {
            var (success, message) = await _subjects.AssignSubjectStaffToSubjectAsync(SubjectId, subjectStaffId,staffRole);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message });
        }
        // remove student from Subject -> move to default General
        [HttpPost("removeStaff/{subjectStaffId}")]
        public async Task<IActionResult> RemovesubjectStaff(int subjectStaffId)
        {
            var (success, message) = await _subjects.RemoveSubjectStaffFromSubjectAsync(subjectStaffId);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message });
        }

    }
}
