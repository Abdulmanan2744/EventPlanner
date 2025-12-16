using Common.Models;
using Common.Models.Planner;
using Common.Models.UserModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Data
{
    public class EventPlannerDbContext
       : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public EventPlannerDbContext(DbContextOptions<EventPlannerDbContext> options)
            : base(options)
        {
        }

        public DbSet<PlannerProfile> PlannerProfiles => Set<PlannerProfile>();
        public DbSet<VerificationDocument> VerificationDocuments => Set<VerificationDocument>();
        public DbSet<GalleryAlbum> GalleryAlbums => Set<GalleryAlbum>();
        public DbSet<GalleryImage> GalleryImages => Set<GalleryImage>();
        public DbSet<DishCategory> DishCategories => Set<DishCategory>();
        public DbSet<Dish> Dishes => Set<Dish>();
        public DbSet<DishVariant> DishVariants => Set<DishVariant>();
        public DbSet<PrebuiltPlan> PrebuiltPlans => Set<PrebuiltPlan>();
        public DbSet<PlanCategory> PlanCategories => Set<PlanCategory>();
        public DbSet<PlanCategoryDishVariant> PlanCategoryDishVariants => Set<PlanCategoryDishVariant>();
        public DbSet<CustomOption> CustomOptions => Set<CustomOption>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventCustomOption> EventCustomOptions => Set<EventCustomOption>();
        public DbSet<EventDishVariant> EventDishVariants => Set<EventDishVariant>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Complaint> Complaints => Set<Complaint>();
        public DbSet<AdminDelegation> AdminDelegations => Set<AdminDelegation>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. PlannerProfile - User (One-to-One)
            builder.Entity<PlannerProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.PlannerProfile)
                .HasForeignKey<PlannerProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. VerificationDocument - User
            builder.Entity<VerificationDocument>()
                .HasOne(vd => vd.User)
                .WithMany(u => u.VerificationDocuments)
                .HasForeignKey(vd => vd.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. AdminDelegation - Two relationships to AppUser
            builder.Entity<AdminDelegation>()
                .HasOne(ad => ad.Admin)
                .WithMany()
                .HasForeignKey(ad => ad.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AdminDelegation>()
                .HasOne(ad => ad.Delegate)
                .WithMany(u => u.Delegations)
                .HasForeignKey(ad => ad.DelegateId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Review - Two relationships to AppUser
            builder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Reviewee)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Event - Two relationships to AppUser
            builder.Entity<Event>()
                .HasOne(e => e.Planner)
                .WithMany(u => u.PlannedEventsAsPlanner)
                .HasForeignKey(e => e.PlannerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Event>()
                .HasOne(e => e.Customer)
                .WithMany(u => u.EventsAsCustomer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Complaint relationships
            builder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.ComplaintsFiled)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Complaint>()
                .HasOne(c => c.AgainstUser)
                .WithMany()
                .HasForeignKey(c => c.AgainstUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // 7. Composite Keys
            builder.Entity<PlanCategoryDishVariant>()
                .HasKey(x => new { x.PlanCategoryId, x.DishVariantId });

            builder.Entity<EventCustomOption>()
                .HasKey(x => new { x.EventId, x.CustomOptionId });

            builder.Entity<EventDishVariant>()
                .HasKey(x => new { x.EventId, x.DishVariantId });

            // 8. FIX: PlanCategoryDishVariant relationships - Prevent cascade cycles
            builder.Entity<PlanCategoryDishVariant>()
                .HasOne(pcdv => pcdv.PlanCategory)
                .WithMany(pc => pc.SelectedVariants)
                .HasForeignKey(pcdv => pcdv.PlanCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PlanCategoryDishVariant>()
                .HasOne(pcdv => pcdv.DishVariant)
                .WithMany(dv => dv.PlanCategories)
                .HasForeignKey(pcdv => pcdv.DishVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // 9. EventDishVariant relationships - Prevent cascade cycles
            builder.Entity<EventDishVariant>()
                .HasOne(edv => edv.Event)
                .WithMany()
                .HasForeignKey(edv => edv.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EventDishVariant>()
                .HasOne(edv => edv.DishVariant)
                .WithMany(dv => dv.EventSelections)
                .HasForeignKey(edv => edv.DishVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // 10. EventCustomOption relationships - Prevent cascade cycles
            builder.Entity<EventCustomOption>()
                .HasOne(eco => eco.Event)
                .WithMany(e => e.SelectedCustomOptions)
                .HasForeignKey(eco => eco.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EventCustomOption>()
                .HasOne(eco => eco.CustomOption)
                .WithMany()
                .HasForeignKey(eco => eco.CustomOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}