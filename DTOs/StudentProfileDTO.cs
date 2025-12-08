namespace Graduation_project.DTOs
{
    public class StudentProfileDto
    {
        public string AppUserId { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public int JoinYear { get; set; }
        public int AcademicLevel { get; set; }
        public int CurrentSemester { get; set; } 
        public int ExpectedGradYear { get; set; }
        public int MajorId { get; set; }
        public string? MajorName { get; set; }
        public bool HideJoinYear { get; set; }
        public bool HideMajor { get; set; }
        public bool HideSubjects { get; set; }
    }

    public class StudentProfileUpdateDto
    {
        public string? StudentId { get; set; } 
        public int? JoinYear { get; set; }
        public int? AcademicLevel { get; set; }
        public int? CurrentSemester { get; set; }
        public int? ExpectedGradYear { get; set; }
        public int? MajorId { get; set; } 
        public bool? HideJoinYear { get; set; }
        public bool? HideMajor { get; set; }
        public bool? HideSubjects { get; set; }
    }
}
