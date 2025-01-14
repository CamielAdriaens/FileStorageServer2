using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using MongoDB.Bson;
using System;
using INTERFACES;
using MODELS;
using DTOs;
using Microsoft.AspNetCore.SignalR;

namespace FileStorage.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IUserService _userService;
        private readonly IHubContext<FileSharingHub> _hubContext; // Inject the HubContext

        public FilesController(IFileService fileService, IUserService userService, IHubContext<FileSharingHub> hubContext)
        {
            _fileService = fileService;
            _userService = userService;
            _hubContext = hubContext;
        }

        [HttpGet("secure-files")]
        public async Task<IActionResult> GetUserFiles()
        {
            var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is not provided");

            var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name);

            if (googleId == null)
                return Unauthorized("Google ID not found");

            Console.WriteLine($"Passing to GetOrCreateUserByGoogleIdAsync - Google ID: {googleId}, Email: {email}, Name: {name}");

            var user = await _userService.GetOrCreateUserByGoogleIdAsync(googleId, email, name);

            using var stream = file.OpenReadStream();
            var mongoFileId = await _fileService.UploadFileAsync(stream, file.FileName);

            await _userService.AddUserFileAsync(googleId, mongoFileId.ToString(), file.FileName);

            // Notify all connected clients about the new file upload
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"File uploaded: {file.FileName}");

            return Ok(new { FileId = mongoFileId.ToString() });
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest("Invalid file ID");

            var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFile(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest("Invalid file ID");

            var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (googleId == null)
                return Unauthorized("Google ID not found");

            var userFiles = await _userService.GetUserFilesAsync(googleId);
            var file = userFiles.FirstOrDefault(f => f.MongoFileId == id);

            if (file == null)
                return NotFound("File not found or you do not have permission to access this file.");

            await _fileService.DeleteFileAsync(objectId);
            await _userService.RemoveUserFileAsync(googleId, id);

            return Ok("File Deleted Succesfully");
        }
        [HttpGet("pending-shares")]
        public async Task<IActionResult> GetPendingShares()
        {
            var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (googleId == null)
            {
                Console.WriteLine("Google ID not found in user claims.");
                return Unauthorized("Google ID not found");
            }

            try
            {
                var pendingShares = await _userService.GetPendingSharesAsync(googleId);
                return Ok(pendingShares);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving pending shares for user {googleId}: {ex.Message}");
                return StatusCode(500, "Internal server error while retrieving pending shares.");
            }
        }
        [HttpPost("share-file")]
        public async Task<IActionResult> ShareFile([FromBody] ShareFileRequest request)
        {
            var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (googleId == null)
                return Unauthorized("Google ID not found");

            // Check if the recipient email matches the user's email
            var userEmail = User.FindFirstValue(ClaimTypes.Email); // Assuming the user's email is stored as a claim
            if (userEmail == request.RecipientEmail)
            {
                return BadRequest("You cannot share files with your own email.");
            }

            try
            {
                await _userService.ShareFileAsync(googleId, request.RecipientEmail, request.FileName, request.MongoFileId);

                // Notify all connected clients about the file sharing event
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"File shared: {request.FileName} with {request.RecipientEmail}");

                return Ok("File shared successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("accept-share/{shareId}")]
        public async Task<IActionResult> AcceptShare(int shareId)
        {
            try
            {
                await _userService.AcceptFileShareAsync(shareId);
                return Ok("File share accepted.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("refuse-share/{shareId}")]
        public async Task<IActionResult> RefuseShare(int shareId)
        {
            try
            {
                await _userService.RefuseFileShareAsync(shareId);
                return Ok("File share refused.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
