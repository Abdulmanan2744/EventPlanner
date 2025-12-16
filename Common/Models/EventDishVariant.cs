using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class EventDishVariant
    {
        public Guid EventId { get; set; }
        public Guid DishVariantId { get; set; }
        public int Quantity { get; set; } = 1;

        [ForeignKey(nameof(EventId))]
        public virtual Event Event { get; set; } = default!;

        [ForeignKey(nameof(DishVariantId))]
        public virtual DishVariant DishVariant { get; set; } = default!;
    }
}
