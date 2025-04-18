using System;
using System.ComponentModel.DataAnnotations;

namespace ToDoListAppA2.Models
{
    public class ToDoList
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        public string Status { get; set; } // "Completed", "In Progress", "To Do"

        // Foreign key to User
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
