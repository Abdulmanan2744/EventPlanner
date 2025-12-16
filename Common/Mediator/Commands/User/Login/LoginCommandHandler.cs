using Common.Mediator.Commands.User.Login.Common.Mediator.Commands.User.Login;
using Common.Mediator.DTO;
using Common.Mediator.DTO.API;
using Common.Models.UserModel;
using Common.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.Commands.User.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService; // Move your GenerateJwtToken logic here

        public LoginCommandHandler(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. Find User
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new ApiResponse<LoginResponseDto> { Success = false, Message = "Invalid email or password." };
            }

            // 2. Check Email Confirmation
            if (!user.EmailConfirmed)
            {
                return new ApiResponse<LoginResponseDto> { Success = false, Message = "Please verify your email first." };
            }

            // 3. Check Password & Lockout
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                return new ApiResponse<LoginResponseDto> { Success = false, Message = "Account locked due to multiple failed attempts." };

            if (!result.Succeeded)
                return new ApiResponse<LoginResponseDto> { Success = false, Message = "Invalid email or password." };

            // 4. Success - Prepare Data
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateJwtToken(user, roles);

            return new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful.",
                Data = new LoginResponseDto
                {
                    Token = token,
                    User = new LoginResponseDto.UserData
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FullName = user.FullName,
                        Roles = roles
                    }
                }
            };
        }
    }
}