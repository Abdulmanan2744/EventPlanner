using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTO.VerificationDto
{
    public class FileUploadDto
    {
         // Selfie, IdCard, Passport, Other
        public List<IFormFile> Files { get; set; } = new();
        public string DocumentType { get; set; } = default!;
    }
}
