using Graduation_project.Data;
using GraduationProject.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Graduation_project.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                Context.Abort(); // لو التوكن مش صحيح
                return;
            }

            await base.OnConnectedAsync();
        }

        // Client → Server: Send Message
        public async Task SendMessage(string receiverId, string msg)
        {
            try
            {
                var senderId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(msg))
                    return;

                var message = new Message
                {
                    Content = msg,
                    SentAt = DateTime.UtcNow,
                    IsRead = false,
                    SenderId = senderId,
                    ReceiverId = receiverId
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                var data = new
                {
                    sender_id = senderId,
                    msg = msg,
                    timestamp = message.SentAt,
                    msg_id = message.Id
                };

                await Clients.User(receiverId).SendAsync("ReceiveMessage", data);
                await Clients.Caller.SendAsync("MessageSent", data);
            }
            catch (Exception ex)
            {
                // عشان تشوف الـ error  أثناء الاختبار
                throw new HubException($"Error in SendMessage: {ex.Message}");
            }
        }

        // Client → Server: Mark as Seen
        public async Task MarkAsSeen(int msgId)
        {
            try
            {
                var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return;

                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == msgId && m.ReceiverId == userId);

                if (message != null && !message.IsRead)
                {
                    message.IsRead = true;
                    await _context.SaveChangesAsync();

                    await Clients.User(message.SenderId).SendAsync("MessageSeen", new { msg_id = msgId });
                }
            }
            catch (Exception ex)
            {
                throw new HubException($"Error in MarkAsSeen: {ex.Message}");
            }
        }
    }
}
