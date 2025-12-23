using GraduationProject.Statics;

namespace GraduationProject.Models
{
    public class StudentProfile
    {
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public string StudentId { get; set; }
        public int AcademicLevel { get; set; }
        public SemesterType CurrentSemester { get; set; }
        public int JoinYear { get; set; }
        public int ExpectedGradYear { get; set; }
        public int MajorId { get; set; }
        public Major Major { get; set; }
        public bool HideJoinYear { get; set; }
        public bool HideMajor { get; set; }
        public bool HideSubjects { get; set; }
    }
}