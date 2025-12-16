using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.DTO.VerificationDto
{
    public class VerificationStatusDto
    {
        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsIdVerified { get; set; }
        public bool IsVideoVerified { get; set; }
        public string OverallStatus { get; set; } = "NotVerified";
        public List<VerificationDocumentDto> Documents { get; set; } = new();
    }
}
