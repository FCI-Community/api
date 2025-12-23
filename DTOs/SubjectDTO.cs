using GraduationProject.Models;
using GraduationProject.Statics;

namespace Graduation_project.DTOs
{
    public class SubjectCreateDto
    {
        public string Name { get; set; } = string.Empty;//
        public string Code { get; set; } = string.Empty;//
        public int year { get; set; }//

        public int Semester; //

    }

    public class SubjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;//
        public string Code { get; set; } = string.Empty;//
        public int year { get; set; }//
        public SemesterType Semester; //
        public List<StudentProfile> StudentProfiles { get; set; } 
        public ICollection<SubjectStaff> SubjectStaff { get; set; } 
        //public ICollection<GlobalAnnouncement> GlobalAnnouncements { get; set; } 
    }
}
