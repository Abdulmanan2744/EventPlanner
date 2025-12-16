using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.DTO
{
    public class VerifyCodeRequest
    {
        public string Email { get; set; } = default!;
        public string Code { get; set; } = default!;
    }
}
