namespace GraduationProject.Models
{
    public class Major
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<StudentProfile> StudentProfiles { get; set; }
        public List<GlobalAnnouncement> GlobalAnnouncements { get; set; }
    }
}