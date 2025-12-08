using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Graduation_project.DTOs
{
    public class AdminCreateDto
    {
        [Required]
        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [Required]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
