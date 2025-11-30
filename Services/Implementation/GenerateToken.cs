using DotNetEnv;
using Graduation_project.Services.IService;
using GraduationProject.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Graduation_project.Services.Implementation
{
    public class GenerateToken: IGenerateToken
    {
        private readonly IConfiguration _configuration;

        public GenerateToken(IConfiguration configuration)
        {
            _configuration = configuration;
            Env.Load();
        }
        public string GetAndCreateToken(AppUser user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var tokenSecret = Environment.GetEnvironmentVariable("Token__Secret")
                              ?? _configuration["Token:Secret"];
            var tokenIssuer = Environment.GetEnvironmentVariable("Token__Issuer")
                              ?? _configuration["Token:Issuer"];
            var tokenAudience = Environment.GetEnvironmentVariable("Token__Audience")
                              ?? _configuration["Token:Audience"];

            if (string.IsNullOrWhiteSpace(tokenSecret))
                throw new InvalidOperationException("Token secret is not configured.");

            var key = Encoding.UTF8.GetBytes(tokenSecret);

            SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                Issuer = tokenIssuer,
                Audience = tokenAudience,
                SigningCredentials = signingCredentials,
                NotBefore = DateTime.Now
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
