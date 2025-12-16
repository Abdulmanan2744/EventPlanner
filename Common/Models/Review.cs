using Common.Models.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Review
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ReviewerId { get; set; }
        public Guid RevieweeId { get; set; }
        public int Rating { get; set; }
        [MaxLength(1000)] 
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ReviewerId))] 
        public virtual AppUser Reviewer { get; set; } = default!;
        [ForeignKey(nameof(RevieweeId))] 
        public virtual AppUser Reviewee { get; set; } = default!;
    }
}
