using Common.Data;
using Common.Models.DTO.VerificationDto;
using Common.Models.UserModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UserSideAPI.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EventPlannerDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public VerificationController(
            UserManager<AppUser> userManager,
            EventPlannerDbContext context,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        #region Phone Verification

        // POST: api/Verification/VerifyPhoneNumber
        [HttpPost("VerifyPhoneNumber")]
        public async Task<IActionResult> VerifyPhoneNumber([FromBody] PhoneVerificationDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                // Update phone number
                user.PhoneNumber = model.PhoneNumber;
                user.PhoneNumberConfirmed = false;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { status = "error", message = errors });
                }

                // Generate verification code (In production, send via SMS)
                var code = new Random().Next(100000, 999999).ToString();

                // TODO: Send SMS with code using SMS service
                // For now, return code in response (REMOVE IN PRODUCTION)

                return Ok(new
                {
                    status = "success",
                    message = "Phone number added. Verification code sent.",
                    code = code // Remove this in production
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: api/Verification/ConfirmPhoneNumber
        [HttpPost("ConfirmPhoneNumber")]
        public async Task<IActionResult> ConfirmPhoneNumber([FromBody] ConfirmPhoneDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                // TODO: Verify the code from cache/database
                // For now, we'll just mark as confirmed
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);

                return Ok(new { status = "success", message = "Phone number verified successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        #endregion

        #region ID Card Verification

        // POST: api/Verification/UploadIdCard
        [HttpPost("UploadIdCard")]
        public async Task<IActionResult> UploadIdCard([FromForm] FileUploadDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                if (model.Files == null || model.Files.Count != 2)
                    return BadRequest(new { status = "error", message = "Both front and back images are required." });

                // Validate files
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                List<object> savedFiles = new List<object>();

                for (int i = 0; i < model.Files.Count; i++)
                {
                    var file = model.Files[i];
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                        return BadRequest(new { status = "error", message = "Only JPG, JPEG, and PNG files are allowed." });

                    if (file.Length > 5 * 1024 * 1024)
                        return BadRequest(new { status = "error", message = "File size must be less than 5MB." });

                    // Save file
                    var documentType = i == 0 ? "IdFront" : "IdBack";
                    var fileName = $"{documentType}_{userId}_{DateTime.UtcNow.Ticks}{extension}";
                    var filePath = await SaveFile(file, "idcards", fileName);

                    // Save to database
                    var document = new VerificationDocument
                    {
                        UserId = Guid.Parse(userId),
                        DocumentType = documentType,
                        FilePath = filePath,
                        UploadedAt = DateTime.UtcNow
                    };

                    _context.VerificationDocuments.Add(document);
                    savedFiles.Add(new
                    {
                        documentId = document.Id,
                        documentType = documentType,
                        filePath = filePath
                    });
                }

                await _context.SaveChangesAsync();

                // Update user verification status
                user.VerificationStatus = "UnderReview";
                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    status = "success",
                    message = "ID card uploaded successfully. Your documents are under review.",
                    files = savedFiles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        #endregion

        #region Video Verification

        // POST: api/Verification/UploadVideo
        [HttpPost("UploadVideo")]
        public async Task<IActionResult> UploadVideo([FromForm] FileUploadDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                if (model.Files == null || model.Files.Count == 0)
                    return BadRequest(new { status = "error", message = "No video file uploaded." });

                var file = model.Files[0];

                // Validate video file
                var allowedExtensions = new[] { ".mp4", ".mov", ".avi" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { status = "error", message = "Only MP4, MOV, and AVI files are allowed." });

                if (file.Length > 50 * 1024 * 1024) // 50MB max
                    return BadRequest(new { status = "error", message = "Video size must be less than 50MB." });

                // Save video file
                var fileName = $"video_{userId}_{DateTime.UtcNow.Ticks}{extension}";
                var filePath = await SaveFile(file, "videos", fileName);

                // Save to database
                var document = new VerificationDocument
                {
                    UserId = Guid.Parse(userId),
                    DocumentType = "Video",
                    FilePath = filePath,
                    UploadedAt = DateTime.UtcNow
                };

                _context.VerificationDocuments.Add(document);
                await _context.SaveChangesAsync();

                // Update user verification status
                if (user.VerificationStatus != "Approved")
                {
                    user.VerificationStatus = "UnderReview";
                    await _userManager.UpdateAsync(user);
                }

                return Ok(new
                {
                    status = "success",
                    message = "Video uploaded successfully. Under review.",
                    documentId = document.Id,
                    filePath = filePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        #endregion

        #region Planner Document Verification

        // POST: api/Verification/UploadPlannerDocument
        [HttpPost("UploadPlannerDocument")]
        public async Task<IActionResult> UploadPlannerDocument([FromForm] FileUploadDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                // Check if user has planner role
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Contains("Planner") && !roles.Contains("PendingPlanner"))
                {
                    return BadRequest(new { status = "error", message = "User is not registered as a planner." });
                }

                if (model.Files == null || model.Files.Count == 0)
                    return BadRequest(new { status = "error", message = "No file uploaded." });

                // Allowed extensions for planner documents
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                List<object> savedFiles = new List<object>();

                foreach (var file in model.Files)
                {
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                        return BadRequest(new { status = "error", message = "Invalid file format." });

                    if (file.Length > 10 * 1024 * 1024) // 10MB max
                        return BadRequest(new { status = "error", message = "File size must be less than 10MB." });

                    // Generate file name
                    var fileName = $"{model.DocumentType}_{userId}_{DateTime.UtcNow.Ticks}{extension}";
                    var filePath = await SaveFile(file, "planner-documents", fileName);

                    // Save in database
                    var document = new VerificationDocument
                    {
                        UserId = Guid.Parse(userId),
                        DocumentType = model.DocumentType,
                        FilePath = filePath,
                        UploadedAt = DateTime.UtcNow
                    };

                    _context.VerificationDocuments.Add(document);
                    savedFiles.Add(new
                    {
                        documentId = document.Id,
                        documentType = model.DocumentType,
                        filePath = filePath
                    });
                }

                await _context.SaveChangesAsync();

                // Update user verification status
                user.VerificationStatus = "UnderReview";
                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    status = "success",
                    message = "Document(s) uploaded successfully. Under review.",
                    files = savedFiles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        #endregion

        #region Get Verification Data

        // GET: api/Verification/GetDocuments
        [HttpGet("GetDocuments")]
        public async Task<IActionResult> GetDocuments()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var documents = await _context.VerificationDocuments
                    .Where(d => d.UserId == Guid.Parse(userId))
                    .Select(d => new
                    {
                        id = d.Id,
                        documentType = d.DocumentType,
                        filePath = d.FilePath,
                        uploadedAt = d.UploadedAt
                    })
                    .ToListAsync();

                return Ok(new { status = "success", data = documents });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: api/Verification/GetVerificationStatus
        [HttpGet("GetVerificationStatus")]
        public async Task<IActionResult> GetVerificationStatus()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                var documents = await _context.VerificationDocuments
                    .Where(d => d.UserId == Guid.Parse(userId))
                    .ToListAsync();

                var response = new
                {
                    isPhoneVerified = user.PhoneNumberConfirmed,
                    isEmailVerified = user.EmailConfirmed,
                    isIdVerified = documents.Any(d => d.DocumentType == "IdFront" || d.DocumentType == "IdBack"),
                    isVideoVerified = documents.Any(d => d.DocumentType == "Video"),
                    overallStatus = user.VerificationStatus ?? "NotVerified",
                    documents = documents.Select(d => new
                    {
                        id = d.Id,
                        documentType = d.DocumentType,
                        filePath = d.FilePath,
                        uploadedAt = d.UploadedAt
                    })
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        #endregion

        #region Admin Actions

        // POST: api/Verification/ApproveUser (Admin only)
        [HttpPost("ApproveUser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                user.IsVerified = true;
                user.VerificationStatus = "Approved";
                await _userManager.UpdateAsync(user);

                return Ok(new { status = "success", message = "User verified successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: api/Verification/RejectUser (Admin only)
        [HttpPost("RejectUser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectUser(string userId, [FromBody] string reason)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                user.IsVerified = false;
                user.VerificationStatus = $"Rejected: {reason}";
                await _userManager.UpdateAsync(user);

                return Ok(new { status = "success", message = "User verification rejected." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        #endregion

        #region Helper Methods

        private async Task<string> SaveFile(IFormFile file, string folder, string fileName)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folder}/{fileName}";
        }

        #endregion
    }
}
