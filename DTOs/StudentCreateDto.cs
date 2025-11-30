using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Graduation_project.DTOs
{
    public class StudentCreateDto
    {
        [Required]
        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required]
        [JsonPropertyName("join_year")]
        public int JoinYear { get; set; }

        [JsonPropertyName("cover_image")]
        public IFormFile? CoverImage { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }
}
