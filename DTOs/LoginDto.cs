using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Graduation_project.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Required]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
