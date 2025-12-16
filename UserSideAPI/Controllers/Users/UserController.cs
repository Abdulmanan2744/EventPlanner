using Common.Models.DTO;
using Common.Models.UserModel;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UserSideAPI.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public UserController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/User/Profile
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        id = user.Id,
                        email = user.Email,
                        fullName = user.FullName,
                        city = user.City,
                        phoneNumber = user.PhoneNumber,
                        isVerified = user.IsVerified,
                        verificationStatus = user.VerificationStatus,
                        roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // PUT: api/User/UpdateProfile
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                user.FullName = model.FullName ?? user.FullName;
                user.City = model.City ?? user.City;
                user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { status = "error", message = errors });
                }

                return Ok(new { status = "success", message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // DELETE: api/User/DeleteAccount
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { status = "error", message = errors });
                }

                return Ok(new { status = "success", message = "Account deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: api/User/GetAllUsers (Admin only)
        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = _userManager.Users
                    .Select(u => new
                    {
                        id = u.Id,
                        email = u.Email,
                        fullName = u.FullName,
                        city = u.City,
                        isVerified = u.IsVerified,
                        verificationStatus = u.VerificationStatus
                    })
                    .ToList();

                return Ok(new { status = "success", data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: api/User/GetUserById/{id} (Admin only)
        [HttpGet("GetUserById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    status = "success",
                    data = new
                    {
                        id = user.Id,
                        email = user.Email,
                        fullName = user.FullName,
                        city = user.City,
                        phoneNumber = user.PhoneNumber,
                        isVerified = user.IsVerified,
                        verificationStatus = user.VerificationStatus,
                        roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
