namespace Graduation_project.DTOs
{
    public class UserListQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Role { get; set; }

        public int? MajorId { get; set; }

        // join year for students
        public int? JoinYear { get; set; }

        public int? SubjectId { get; set; }

        public string? Search { get; set; }
    }
}
