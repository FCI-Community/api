namespace Graduation_project.DTOs
{
    public class UserListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }

        // student-specific
        public string? StudentId { get; set; }
        public int? MajorId { get; set; }
        public int? JoinYear { get; set; }
    }
}
