using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.DTO.VerificationDto
{
    public class IdCardUploadDto
    {
        public IFormFile FrontImage { get; set; } = default!;
        public IFormFile BackImage { get; set; } = default!;
    }
}
