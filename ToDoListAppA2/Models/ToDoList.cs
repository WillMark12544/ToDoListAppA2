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

        public ICollection<ToDoListNode> ToDoListNodes { get; set; }
        public ICollection<ToDoListShare> SharedWith {  get; set; }

        // Assosiate ToDoList with user
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
    }
}
