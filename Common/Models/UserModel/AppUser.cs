using Common.Models.Planner;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.UserModel
{
    public class AppUser : IdentityUser<Guid>
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = default!;

        [MaxLength(50)]
        public string? City { get; set; }

        // Admin login key
        public bool IsAdminKeyVerified { get; set; } = false;

        // Verification
        public bool IsVerified { get; set; } = false;
        public string VerificationStatus { get; set; } = "Pending"; // Pending, Approved, Rejected

        // Navigation
        public virtual PlannerProfile? PlannerProfile { get; set; }
        public virtual ICollection<VerificationDocument> VerificationDocuments { get; set; } = new HashSet<VerificationDocument>();
        public virtual ICollection<Event> PlannedEventsAsPlanner { get; set; } = new HashSet<Event>();
        public virtual ICollection<Event> EventsAsCustomer { get; set; } = new HashSet<Event>();
        public virtual ICollection<Review> ReviewsGiven { get; set; } = new HashSet<Review>();
        public virtual ICollection<Review> ReviewsReceived { get; set; } = new HashSet<Review>();
        public virtual ICollection<Complaint> ComplaintsFiled { get; set; } = new HashSet<Complaint>();
        public virtual ICollection<AdminDelegation> Delegations { get; set; } = new HashSet<AdminDelegation>();
    }
}
