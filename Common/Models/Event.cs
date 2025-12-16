using Common.Models.Enum;
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
    public class Event
    {
        [Key] public Guid Id { get; set; }
        public Guid PlannerId { get; set; }
        public Guid CustomerId { get; set; }
        [Required, MaxLength(100)] public string EventType { get; set; } = default!;
        public DateTime EventDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public bool IsPending { get; set; } = true;
        public PlanType PlanType { get; set; }

        [ForeignKey(nameof(PlannerId))] public virtual AppUser Planner { get; set; } = default!;
        [ForeignKey(nameof(CustomerId))] public virtual AppUser Customer { get; set; } = default!;

        public virtual ICollection<EventCustomOption> SelectedCustomOptions { get; set; } = new HashSet<EventCustomOption>();
    }
}
