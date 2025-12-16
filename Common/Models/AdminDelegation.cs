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
    public class AdminDelegation
    {
        [Key] public Guid Id { get; set; }
        public Guid AdminId { get; set; }
        public Guid DelegateId { get; set; }
        [Required, MaxLength(100)] public string Permission { get; set; } = default!;
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(AdminId))] public virtual AppUser Admin { get; set; } = default!;
        [ForeignKey(nameof(DelegateId))] public virtual AppUser Delegate { get; set; } = default!;
    }
}
