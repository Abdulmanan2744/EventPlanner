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
    public class Complaint
    {
        [Key] 
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? AgainstUserId { get; set; }
        [Required, MaxLength(2000)] 
        public string Description { get; set; } = default!;
        public DateTime FiledAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;

        [ForeignKey(nameof(UserId))] 
        public virtual AppUser User { get; set; } = default!;
        [ForeignKey(nameof(AgainstUserId))] 
        public virtual AppUser? AgainstUser { get; set; }
    }
}
