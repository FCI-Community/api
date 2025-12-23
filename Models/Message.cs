namespace GraduationProject.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public AppUser Sender { get; set; } = new();
        public string ReceiverId { get; set; } = string.Empty;
        public AppUser Receiver { get; set; }= new();
    }
}