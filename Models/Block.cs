namespace GraduationProject.Models
{
    public class Block
    {
        public int Id { get; set; }
        public string BlockerId { get; set; } 
        public AppUser Blocker { get; set; }
        public string BlockedId { get; set; } 
        public AppUser Blocked { get; set; }

    }
}