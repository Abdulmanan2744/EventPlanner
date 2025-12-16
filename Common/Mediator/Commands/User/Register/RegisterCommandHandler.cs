using Common.Mediator.DTO.API;
using Common.Models.UserModel;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed; // For handling verification codes
using NETCore.MailKit.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Mediator.Commands.User.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<string>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IDistributedCache _cache; // Replaces the local dictionary

        public RegisterCommandHandler(
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IDistributedCache cache)
        {
            _userManager = userManager;
            _emailService = emailService;
            _cache = cache;
        }

        public async Task<ApiResponse<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Registration failed.",
                        Error = "User with this email already exists."
                    };
                }

                // 2. Map Command to Entity
                var user = new AppUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    City = request.City,
                    EmailConfirmed = false,
                    IsVerified = false,
                    VerificationStatus = "Pending"
                };

                // 3. Create User in Database
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "User creation failed.",
                        Error = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                // 4. Assign Role (Default to Customer)
                await _userManager.AddToRoleAsync(user, request.Role ?? "Customer");

                // 5. Generate and Store Verification Code
                // We use Cache because Handlers are stateless
                var code = new Random().Next(100000, 999999).ToString();
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };

                // Store code with Email as key
                await _cache.SetStringAsync($"Verify_{request.Email}", code, cacheOptions, cancellationToken);

                // 6. Send Email
                await _emailService.SendAsync(request.Email, code, "test");

                return new ApiResponse<string>
                {
                    Success = true,
                    Message = "Registration successful.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "An unexpected error occurred during registration.",
                    Error = ex.Message
                };
            }
        }
    }
}