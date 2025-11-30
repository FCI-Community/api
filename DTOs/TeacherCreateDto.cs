using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Graduation_project.DTOs
{
    public class TeacherCreateDto
    {
        [Required]
        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("cover_image")]
        public IFormFile? CoverImage { get; set; }

        [Required]
        [JsonPropertyName("role")]
        public string Role { get; set; }

    }
}
