using System.Text.Json.Serialization;

namespace Graduation_project.DTOs
{
    public class LoginResponseDto
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("profile")]
        public object Profile { get; set; }
    }
}
