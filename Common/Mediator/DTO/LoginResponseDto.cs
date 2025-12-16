using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.DTO
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = default!;
        public UserData User { get; set; } = default!;

        public class UserData
        {
            public Guid Id { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string FullName { get; set; } = default!;
            public IList<string> Roles { get; set; } = default!;
        }
    }
}
