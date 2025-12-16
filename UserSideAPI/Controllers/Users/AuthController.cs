using Common.Models.DTO;
using Common.Models.UserModel;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;

namespace UserSideAPI.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private static Dictionary<string, string> VerificationCodes = new Dictionary<string, string>();
        private static Dictionary<string, DateTime> CodeExpiry = new Dictionary<string, DateTime>();

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // POST: api/Auth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { status = "error", message = "Invalid data provided." });

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                    return BadRequest(new { status = "error", message = "User with this email already exists." });

                // Create new user
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    City = model.City,
                    EmailConfirmed = false,
                    IsVerified = false,
                    VerificationStatus = "Pending"
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { status = "error", message = errors });
                }

                // Assign role (default: Customer)
                await _userManager.AddToRoleAsync(user, model.Role ?? "Customer");

                // Send verification email
                var code = new Random().Next(100000, 999999).ToString();
                VerificationCodes[model.Email] = code;
                CodeExpiry[model.Email] = DateTime.UtcNow.AddMinutes(10);
                await SendVerificationEmail(model.Email, code);

                return Ok(new
                {
                    status = "success",
                    message = "Registration successful. Please verify your email.",
                    userId = user.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { status = "error", message = "Invalid credentials." });

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return Unauthorized(new { status = "error", message = "Invalid email or password." });

                // Check if email is confirmed
                if (!user.EmailConfirmed)
                    return Unauthorized(new { status = "error", message = "Please verify your email first." });

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                        return Unauthorized(new { status = "error", message = "Account locked due to multiple failed attempts." });

                    return Unauthorized(new { status = "error", message = "Invalid email or password." });
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);

                // Generate JWT token
                var token = GenerateJwtToken(user, roles);

                return Ok(new
                {
                    status = "success",
                    token = token,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        fullName = user.FullName,
                        roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: api/Auth/SendVerificationCode
        [HttpPost("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode([FromBody] SendCodeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                    return BadRequest(new { status = "error", message = "Email is required." });

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                string code = new Random().Next(100000, 999999).ToString();
                VerificationCodes[request.Email] = code;
                CodeExpiry[request.Email] = DateTime.UtcNow.AddMinutes(10);

                await SendVerificationEmail(request.Email, code);

                return Ok(new { status = "success", message = "Verification code sent to your email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"Failed to send code: {ex.Message}" });
            }
        }

        // POST: api/Auth/VerifyCode
        [HttpPost("VerifyCode")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            try
            {
                if (!VerificationCodes.ContainsKey(request.Email))
                    return BadRequest(new { status = "error", message = "No verification code found for this email." });

                if (DateTime.UtcNow > CodeExpiry[request.Email])
                {
                    VerificationCodes.Remove(request.Email);
                    CodeExpiry.Remove(request.Email);
                    return BadRequest(new { status = "error", message = "Verification code has expired." });
                }

                if (VerificationCodes[request.Email] != request.Code)
                    return BadRequest(new { status = "error", message = "Invalid verification code." });

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }

                VerificationCodes.Remove(request.Email);
                CodeExpiry.Remove(request.Email);

                return Ok(new { status = "success", message = "Email verified successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: api/Auth/ForgotPassword
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] Common.Models.DTO.ForgotPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                string code = new Random().Next(100000, 999999).ToString();
                VerificationCodes[request.Email] = code;
                CodeExpiry[request.Email] = DateTime.UtcNow.AddMinutes(10);

                await SendPasswordResetEmail(request.Email, code);

                return Ok(new { status = "success", message = "Password reset code sent to your email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: api/Auth/ResetPassword
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] Common.Models.DTO.ResetPasswordRequest request)
        {
            try
            {
                if (!VerificationCodes.ContainsKey(request.Email))
                    return BadRequest(new { status = "error", message = "No reset code found." });

                if (DateTime.UtcNow > CodeExpiry[request.Email])
                {
                    VerificationCodes.Remove(request.Email);
                    CodeExpiry.Remove(request.Email);
                    return BadRequest(new { status = "error", message = "Reset code has expired." });
                }

                if (VerificationCodes[request.Email] != request.Code)
                    return BadRequest(new { status = "error", message = "Invalid reset code." });

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { status = "error", message = errors });
                }

                VerificationCodes.Remove(request.Email);
                CodeExpiry.Remove(request.Email);

                return Ok(new { status = "success", message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: api/Auth/ChangePassword
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound(new { status = "error", message = "User not found." });

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { status = "error", message = errors });
                }

                return Ok(new { status = "success", message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = $"An error occurred: {ex.Message}" });
            }
        }

        #region Helper Methods

        private string GenerateJwtToken(AppUser user, IList<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Issuer"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task SendVerificationEmail(string email, string code)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("aquib.ch.77@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Email Verification Code";
                mail.Body = $"Your verification code is: {code}. This code will expire in 10 minutes.";

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential("aquib.ch.77@gmail.com", "wrhk pefr ubml sqqo");
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}");
            }
        }

        private async Task SendPasswordResetEmail(string email, string code)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("aquib.ch.77@gmail.com");
                mail.To.Add(email);
                mail.Subject = "Password Reset Code";
                mail.Body = $"Your password reset code is: {code}. This code will expire in 10 minutes.";

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new System.Net.NetworkCredential("aquib.ch.77@gmail.com", "wrhk pefr ubml sqqo");
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}");
            }
        }

        #endregion
    }

}
