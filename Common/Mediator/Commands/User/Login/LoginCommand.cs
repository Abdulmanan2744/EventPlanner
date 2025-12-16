using Common.Mediator.DTO;
using Common.Mediator.DTO.API;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.Commands.User.Login
{
    namespace Common.Mediator.Commands.User.Login
    {
        public class LoginCommand : IRequest<ApiResponse<LoginResponseDto>>
        {
            public required string Email { get; set; }
            public required string Password { get; set; }
        }
    }
}
