using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.DTO
{
    public class RegisterDto
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? City { get; set; }
        public string? Role { get; set; } = "Customer"; // Customer, Planner
    }
}
