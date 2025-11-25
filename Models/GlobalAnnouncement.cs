namespace GraduationProject.Models
{
    public class GlobalAnnouncement
    {
        public int Id { get; set; }
        public string Title { get; set; } //= string.Empty;
        public string Content { get; set; }// = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Major> TargetMajors { get; set; }

        public List<Subject> TargetSubjects { get; set; }
    }
}