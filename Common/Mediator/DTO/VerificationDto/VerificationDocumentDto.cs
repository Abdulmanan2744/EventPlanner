using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Mediator.DTO.VerificationDto
{
    public class VerificationDocumentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
