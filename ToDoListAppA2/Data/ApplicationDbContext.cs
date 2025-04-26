using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ToDoList> ToDoLists { get; set; }
        public DbSet<ToDoListNode> ToDoListNodes { get; set; }
        public DbSet<ToDoListShare> ToDoListShares { get; set; }

        // Define relationship between models
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ToDoList to ToDoListNode relationship
            builder.Entity<ToDoList>()
                .HasMany(t => t.ToDoListNodes)
                .WithOne(n => n.ToDoList)
                .HasForeignKey(n => n.ToDoListId)
                .OnDelete(DeleteBehavior.Cascade); // Remove all ToDoListNodes when associated ToDoList is deleted

            // ToDoList to ToDoListShare relationship
            builder.Entity<ToDoList>()
                .HasMany(t => t.SharedWith)
                .WithOne(s => s.ToDoList)
                .HasForeignKey(s => s.ToDoListId)
                .OnDelete(DeleteBehavior.Cascade); // Remove all ToDoListShares when associated ToDoList is deleted

            // ToDoListShare to ApplicationUser relationship
            builder.Entity<ToDoListShare>()
                .HasOne(s => s.SharedWithUser)
                .WithMany()
                .HasForeignKey(s => s.SharedWithUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of user if referenced in ToDoListShare (not required as users won't be deletable)

            // ApplicationUser to ToDoList relationship
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.ToDoLists)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Remove all ToDoLists of ApplicationUser when assosiated user is deleted
        }
    }
}
