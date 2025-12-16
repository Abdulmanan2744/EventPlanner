using Common.Models.Enum;
using Common.Models.Planner;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class CustomOption
    {
        [Key] 
        public Guid Id { get; set; }
        public Guid PlannerProfileId { get; set; }
        public OptionType Type { get; set; } // Food, Location, Decoration, Catering
        [Required, MaxLength(150)] 
        public string Name { get; set; } = default!;
        [MaxLength(500)] 
        public string? Description { get; set; }
        public decimal Rate { get; set; }
        public bool IsPerPerson { get; set; }

        [ForeignKey(nameof(PlannerProfileId))]
        public virtual PlannerProfile PlannerProfile { get; set; } = default!;

    }
}
