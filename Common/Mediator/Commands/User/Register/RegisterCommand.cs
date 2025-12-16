using Common.Mediator.DTO.API;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.Commands.User.Register
{
    public class RegisterCommand : IRequest<ApiResponse<string>>
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string City { get; set; }
        public required string Password { get; set; }
        public required string? Role { get; set; }
    }
}
