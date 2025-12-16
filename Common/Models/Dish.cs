using Common.Models.Planner;
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
    public class Dish
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PlannerProfileId { get; set; }           // Who added this dish
        public Guid DishCategoryId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = default!;         // "Coca Cola", "Nestle Water", "Chicken Karahi"

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDrink { get; set; } = false;           // Special flag for drinks

        [ForeignKey(nameof(PlannerProfileId))]
        public virtual PlannerProfile Planner { get; set; } = default!;

        [ForeignKey(nameof(DishCategoryId))]
        public virtual DishCategory Category { get; set; } = default!;

        // One dish → many variants (sizes + packing)
        public virtual ICollection<DishVariant> Variants { get; set; } = new HashSet<DishVariant>();
    }
}
