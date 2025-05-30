using ToDoListAppA2.Models;

namespace ToDoListAppA2.Views.ViewModels
{
    public class ToDoListIndexViewModel
    {
        public IEnumerable<ToDoList> UnarchivedLists { get; set; }
        public IEnumerable<ToDoList> ArchivedLists {  get; set; }
    }
}
