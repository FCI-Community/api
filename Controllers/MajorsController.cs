using Graduation_project.DTOs;
using Graduation_project.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class MajorsController : ControllerBase
    {
        private readonly IMajorRepository _majors;

        public MajorsController(IMajorRepository majors)
        {
            _majors = majors;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _majors.GetAllMajorsAsync();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var major = await _majors.GetMajorByIdAsync(id);
            if (major == null) return NotFound();
            return Ok(major);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MajorCreateDto dto)
        {
            var (success, major, message) = await _majors.CreateMajorAsync(dto);
            if (!success) return BadRequest(new { success = false, message });
            return CreatedAtAction(nameof(Get), new { id = major!.Id }, major);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MajorUpdateDto dto)
        {
            var (success, major, message) = await _majors.UpdateMajorAsync(id, dto);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(major);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _majors.DeleteMajorAsync(id);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success, message });
        }

        // assign student to major
        [HttpPost("{majorId:int}/assign/{userId}")]
        public async Task<IActionResult> AssignStudent(int majorId, string userId)
        {
            var (success, message) = await _majors.AssignStudentToMajorAsync(majorId, userId);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true , message });
        }

        // move student to another major
        [HttpPost("{majorId:int}/move/{userId}")]
        public async Task<IActionResult> MoveStudent(int majorId, string userId)
        {
            var (success, message) = await _majors.MoveStudentToMajorAsync(majorId, userId);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true , message });
        }

        // remove student from major -> move to default General
        [HttpPost("remove/{userId}")]
        public async Task<IActionResult> RemoveStudent(string userId)
        {
            var (success, message) = await _majors.RemoveStudentFromMajorAsync(userId);
            if (!success) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message });
        }
    }
}
