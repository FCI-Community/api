namespace GraduationProject.Models
{
    public class Major
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = "General";
        public List<StudentProfile> StudentProfiles { get; set; } = new();
        public List<GlobalAnnouncement> GlobalAnnouncements { get; set; } = new();
    }
}