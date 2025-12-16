using Common.Models.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class DishCategory
    {
        [Key] 
        public Guid Id { get; set; }
        [Required, MaxLength(100)] 
        public string Name { get; set; } = default!; // Main Course, Dessert, Drinks, etc.
        public virtual ICollection<Dish> Dishes { get; set; } = new HashSet<Dish>();
    }
}
