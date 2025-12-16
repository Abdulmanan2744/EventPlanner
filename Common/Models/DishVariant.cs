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
    public class DishVariant
    {
        [Key]
        public Guid Id { get; set; }

        public Guid DishId { get; set; }

        [Required, MaxLength(50)]
        public string Size { get; set; } = default!;         // "1.5 Litre", "300 ml Can", "500 ml", "2.25 Litre"

        public int BottlesPerCrate { get; set; } = 1;         // 6 for 1.5L, 4 for 2.25L, 24 for 300ml cans

        public decimal PricePerUnit { get; set; }            // Price of 1 unit (bottle or crate)

        public bool IsPerPerson { get; set; } = false;       // e.g., Chicken Karahi = per person

        [ForeignKey(nameof(DishId))]
        public virtual Dish Dish { get; set; } = default!;

        // Used in plans & bookings
        public virtual ICollection<PlanCategoryDishVariant> PlanCategories { get; set; } = new HashSet<PlanCategoryDishVariant>();
        public virtual ICollection<EventDishVariant> EventSelections { get; set; } = new HashSet<EventDishVariant>();
    }
}
