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
    public class PlannerProfile
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? ProfileImagePath { get; set; }
        public int CompletedProjects { get; set; } = 0;
        public int PendingProjects { get; set; } = 0;
        public int AvailableSlots { get; set; } = 0;
        public double AverageRating { get; set; } = 0.0;

        [ForeignKey(nameof(UserId))]
        public virtual AppUser User { get; set; } = default!;

        public virtual ICollection<GalleryAlbum> GalleryAlbums { get; set; } = new HashSet<GalleryAlbum>();
        public virtual ICollection<PrebuiltPlan> PrebuiltPlans { get; set; } = new HashSet<PrebuiltPlan>();
        public virtual ICollection<CustomOption> CustomOptions { get; set; } = new HashSet<CustomOption>();
    }
}
