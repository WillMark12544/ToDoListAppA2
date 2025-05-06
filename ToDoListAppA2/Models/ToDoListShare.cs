using System.ComponentModel.DataAnnotations.Schema;

namespace ToDoListAppA2.Models
{
    public class ToDoListShare
    {
        public int Id { get; set; }

        // Foriegn key to ToDoList
        [ForeignKey("ToDoListId")]
        public ToDoList? ToDoList { get; set; }
        public int ToDoListId { get; set; }


        // Foriegn key to ApplicationUser
        [ForeignKey("SharedWithUserId")]
        public ApplicationUser? SharedWithUser { get; set; }
        public string SharedWithUserId { get; set; }
    }
}
