using Common.Models.Enum;
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
    public class PrebuiltPlan
    {
        [Key] 
        public Guid Id { get; set; }
        public Guid PlannerProfileId { get; set; }
        public PlanType PlanType { get; set; } // Silver, Gold, Platinum

        [ForeignKey(nameof(PlannerProfileId))]
        public virtual PlannerProfile PlannerProfile { get; set; } = default!;

        public virtual ICollection<PlanCategory> Categories { get; set; } = new HashSet<PlanCategory>();
    }
}
