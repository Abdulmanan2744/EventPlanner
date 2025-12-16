using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.UserModel
{
    public class VerificationDocument
    {
        [Key] 
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [Required, MaxLength(50)] 
        public string DocumentType { get; set; } = default!; // LiveSelfie, IdFront, IdBack, Certificate
        [Required] 
        public string FilePath { get; set; } = default!;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual AppUser User { get; set; } = default!;
    }
}
