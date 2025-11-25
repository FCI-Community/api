using GraduationProject.Statics;

namespace GraduationProject.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }//= string.Empty;
        public string Code { get; set; }//= string.Empty;
        public SemesterType Semester { get; set; }
        public List<SubjectStaff> SubjectStaff { get; set; } = new();

        public List<GlobalAnnouncement> GlobalAnnouncements { get; set; } = new();
    }
}