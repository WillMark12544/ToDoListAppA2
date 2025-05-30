using ToDoListAppA2.Models;

namespace ToDoListAppA2.Views.ViewModels
{
    public class ToDoListShareIndexViewModel
    {
        public IEnumerable<ToDoList> UnarchivedSharedLists { get; set; }
        public IEnumerable<ToDoList> ArchivedSharedLists { get; set; }
    }
}
