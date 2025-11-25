using GraduationProject.Statics;

namespace GraduationProject.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }

        public string Title { get; set; } //= string.Empty;
        public string? Message { get; set; }
        public TicketStatus Status { get; set; }

        public string AppUserId { get; set; }// = string.Empty;
        public AppUser AppUser { get; set; }
    }
}