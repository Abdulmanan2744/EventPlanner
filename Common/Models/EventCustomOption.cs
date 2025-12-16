using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class EventCustomOption
    {
        public Guid EventId { get; set; }
        public Guid CustomOptionId { get; set; }

        [ForeignKey(nameof(EventId))] public virtual Event Event { get; set; } = default!;
        [ForeignKey(nameof(CustomOptionId))] public virtual CustomOption CustomOption { get; set; } = default!;
    }
}
