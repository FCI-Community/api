using GraduationProject.Statics;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Models
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public Gender gender { get; set; }
        public StudentProfile? StudentProfile { get; set; }

        public List<Message> SentMessages { get; set; }
        public List<Message> ReceivedMessages { get; set; }

        public List<Block> BlocksInitiated { get; set; } 

        public List<Block> BlocksReceived { get; set; }

        public List<SubjectStaff> SubjectsStaff { get; set; } 
        public List<SupportTicket> SupportTickets { get; set; }
    }

}