using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FileStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
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