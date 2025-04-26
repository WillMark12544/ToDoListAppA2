using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ToDoListAppA2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<ToDoList> ToDoLists { get; set; } // ToDoLists associated with given user
    }

}


