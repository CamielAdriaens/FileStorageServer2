using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _jwtSecretKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public AuthController(IConfiguration configuration)
        {
            _jwtSecretKey = configuration["JwtSettings:SecretKey"];
            _jwtIssuer = configuration["JwtSettings:Issuer"];
            _jwtAudience = configuration["JwtSettings:Audience"];
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleTokenRequest request)
        {
            try
            {
                var clientId = "911031744599-l50od06i5t89bmdl4amjjhdvacsdonm7.apps.googleusercontent.com"; // Replace with your actual Google Client ID

                // Validate the Google ID token
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });

                // Generate a custom JWT token for the application
                var jwtToken = GenerateJwtToken(payload.Subject, payload.Email);

                return Ok(new
                {
                    Message = "Authentication successful",
                    UserId = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture,
                    Jwt = jwtToken
                });
            }
            catch (InvalidJwtException)
            {
                return BadRequest(new { Error = "Invalid Google token" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal Server Error", Details = ex.Message });
            }
        }

        private string GenerateJwtToken(string userId, string email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class GoogleTokenRequest
    {
        public string Credential { get; set; }
    }
}
