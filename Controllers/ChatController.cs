using Graduation_project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Graduation_project.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/chat/chats → Array.of {id, user:{name, cover_url}, last_msg}
        [HttpGet("chats")]
        public async Task<IActionResult> GetAllChats()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var chatGroups = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMsg = g.OrderByDescending(m => m.SentAt).FirstOrDefault()
                })
                .ToListAsync();

            var result = new List<object>();

            foreach (var g in chatGroups)
            {
                var user = await _context.Users.FindAsync(g.OtherUserId);
                if (user != null)
                {
                    result.Add(new
                    {
                        id = user.Id,
                        user = new
                        {
                            name = user.FullName,
                            cover_url = user.ProfilePictureUrl
                        },
                        last_msg = g.LastMsg?.Content ?? ""
                    });
                }
            }

            return Ok(result);
        }

        // GET: api/chat/history/{userId}
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetChatHistory(string userId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null) return Unauthorized();

            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == currentUserId && m.ReceiverId == userId) ||          // SENT MSGs => return all
                    (m.ReceiverId == currentUserId && m.SenderId == userId && m.IsRead) // RECEIVED MSGs => return read only
                )
                .OrderByDescending(m => m.SentAt)
                .Take(20)
                .Select(m => new
                {
                    msg_id = m.Id,
                    sender_id = m.SenderId,
                    content = m.Content,
                    timestamp = m.SentAt,
                    isRead = m.IsRead
                })
                .ToListAsync();

            return Ok(messages); // لو مفيش رسائل → array فاضي
        }
    }
}
