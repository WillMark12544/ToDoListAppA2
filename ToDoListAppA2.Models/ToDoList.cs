using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoListAppA2.Models
{
    public class ToDoList
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        // Initialize collections, can be empty
        public ICollection<ToDoListNode> ToDoListNodes { get; set; } = new List<ToDoListNode>();
        public ICollection<ToDoListShare> SharedWith {  get; set; } = new List<ToDoListShare>();

        // Assosiate ToDoList with user
        [ForeignKey("UserId")]
        // Made both User and UserId nullable to avoid model binding errors
        public ApplicationUser? User { get; set; } // Set based off the UserId set in the controller
        public string? UserId { get; set; } // Set in Create() action method of MyToDoListController 
    }
}
