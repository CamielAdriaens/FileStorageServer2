using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LOGIC;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using INTERFACES;


namespace FileStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Google JWT authorization
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService; // MongoDB file service
        private readonly IUserService _userService; // SQL user service

        public FilesController(IFileService fileService, IUserService userService)
        {
            _fileService = fileService;
            _userService = userService;
        }

        [HttpGet("secure-files")]
        public async Task<IActionResult> GetUserFiles()
      {
            var googleId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (googleId == null)
            {
                Console.WriteLine("Google ID not found in user claims.");
                return Unauthorized("Google ID not found");
            }

            try
            {
                var userFiles = await _userService.GetUserFilesAsync(googleId);
                return Ok(userFiles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving files for user {googleId}: {ex.Message}");
                return StatusCode(500, "Internal server error while retrieving user files.");
            }
        }

        // Upload a file to MongoDB and store metadata in SQL
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is not provided");

            var googleId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (googleId == null)
                return Unauthorized("Google ID not found");

            Console.WriteLine($"Passing to GetOrCreateUserByGoogleIdAsync - Google ID: {googleId}, Email: {email}, Name: {name}");

            // Get or create the user
            var user = await _userService.GetOrCreateUserByGoogleIdAsync(googleId, email, name);

            // Store the file in MongoDB
            using var stream = file.OpenReadStream();
            var mongoFileId = await _fileService.UploadFileAsync(stream, file.FileName);

            // Store metadata in SQL
            await _userService.AddUserFileAsync(googleId, mongoFileId.ToString(), file.FileName);

            return Ok(new { FileId = mongoFileId.ToString() });
        }

        // Download a file from MongoDB
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest("Invalid file ID");

            var googleId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (googleId == null)
                return Unauthorized("Google ID not found");

            var userFiles = await _userService.GetUserFilesAsync(googleId);
            var file = userFiles.FirstOrDefault(f => f.MongoFileId == id);

            if (file == null)
                return NotFound("File not found or you do not have permission to access this file.");

            var fileStream = await _fileService.DownloadFileAsync(objectId);
            if (fileStream == null)
                return NotFound("File not found in storage.");

            return File(fileStream, "application/octet-stream", file.FileName);
        }

        // Delete a file from MongoDB and remove metadata from SQL
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFile(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest("Invalid file ID");

            var googleId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (googleId == null)
                return Unauthorized("Google ID not found");

            var userFiles = await _userService.GetUserFilesAsync(googleId);
            var file = userFiles.FirstOrDefault(f => f.MongoFileId == id);

            if (file == null)
                return NotFound("File not found or you do not have permission to access this file.");

            // Delete the file from MongoDB
            await _fileService.DeleteFileAsync(objectId);

            // Remove the file metadata from SQL
            await _userService.RemoveUserFileAsync(googleId, id);

            return NoContent();
        }
    }
}
