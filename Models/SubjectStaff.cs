using GraduationProject.Statics;

namespace GraduationProject.Models
{
    public class SubjectStaff 
    {
        public int Id { get; set; }
        public StaffRole Role { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public AppUser AppUser { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
    }
}