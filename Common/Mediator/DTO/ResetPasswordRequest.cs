using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.DTO
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
