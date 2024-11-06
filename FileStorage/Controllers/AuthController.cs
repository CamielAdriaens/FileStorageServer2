
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using INTERFACES; // For service interfaces only
using LOGIC;      // For service implementations
using DTOs;       // For data transfer objects (if needed in controllers)
using Models;     // If needed for models directly used in responses or validations


namespace FileStorage.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public string GenerateJwtToken(List<Claim> claims)

        {
            // Get JWT settings from appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Set expiration time as needed
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleTokenRequest request)
        {
            try
            {
                var clientId = "911031744599-l50od06i5t89bmdl4amjjhdvacsdonm7.apps.googleusercontent.com"; // Replace with your actual Google Client ID

                // Validate the Google ID token (the credential from the frontend)
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });
                Console.WriteLine($"Google ID: {payload.Subject}, Email: {payload.Email}, Name: {payload.Name}");
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, payload.Subject),
            new Claim(ClaimTypes.Email, payload.Email),
            new Claim(ClaimTypes.Name, payload.Name)  // Add Name as a claim
        };
                var token = GenerateJwtToken(claims);


                // Return success if the token is valid
                return Ok(new
                {
                    Message = "Authentication successful",
                    UserId = payload.Subject, // Google's unique user ID
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture // URL to the user's profile picture
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
    }

    public class GoogleTokenRequest
    {
        public string Credential { get; set; } // This is the token from the frontend (Google ID token)
    }
    

}
