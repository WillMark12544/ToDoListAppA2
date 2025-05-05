using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoListAppA2.Models
{
    public class ToDoListNode
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string? Status { get; set; }
        public DateTime? DueDate { get; set; }

        // Foregin key to ToDoList
        [ForeignKey("ToDoListId")]
        public ToDoList? ToDoList { get; set; }
        public int ToDoListId { get; set; } 


    }
}
