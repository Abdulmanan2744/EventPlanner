using Common.Models.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Planner
{
    public class GalleryAlbum
    {
        [Key]
        public Guid Id { get; set; }
        public Guid PlannerProfileId { get; set; }
        [Required, MaxLength(100)] 
        public string EventType { get; set; } = default!; // Mehndi, Wedding, Birthday

        [ForeignKey(nameof(PlannerProfileId))]
        public virtual PlannerProfile PlannerProfile { get; set; } = default!;

        public virtual ICollection<GalleryImage> Images { get; set; } = new HashSet<GalleryImage>();
    }
}
