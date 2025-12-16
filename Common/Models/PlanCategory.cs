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
    public class PlanCategory
    {
        [Key] public Guid Id { get; set; }
        public Guid PrebuiltPlanId { get; set; }
        public int MaxPeople { get; set; }
        [MaxLength(500)] public string? Description { get; set; }
        public decimal Rate { get; set; }

        [ForeignKey(nameof(PrebuiltPlanId))]
        public virtual PrebuiltPlan PrebuiltPlan { get; set; } = default!;

        // Now uses variants instead of raw dishes
        public virtual ICollection<PlanCategoryDishVariant> SelectedVariants { get; set; } = new HashSet<PlanCategoryDishVariant>();
    }
}
