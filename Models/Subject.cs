using GraduationProject.Statics;

namespace GraduationProject.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;//
        public string Code { get; set; } = string.Empty;//
        public int year { get; set; }//
        public SemesterType Semester { get; set; }//
        public List<StudentProfile> StudentProfiles { get; set; } = new();
        public ICollection<SubjectStaff> SubjectStaff { get; set; }
        public ICollection<GlobalAnnouncement> GlobalAnnouncements { get; set; }
    }
}