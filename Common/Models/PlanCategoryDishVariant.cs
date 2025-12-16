using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class PlanCategoryDishVariant
    {
        public Guid PlanCategoryId { get; set; }
        public Guid DishVariantId { get; set; }

        public int Quantity { get; set; } = 1;               // e.g., 5 full crates

        [ForeignKey(nameof(PlanCategoryId))]
        public virtual PlanCategory PlanCategory { get; set; } = default!;

        [ForeignKey(nameof(DishVariantId))]
        public virtual DishVariant DishVariant { get; set; } = default!;
    }
}
