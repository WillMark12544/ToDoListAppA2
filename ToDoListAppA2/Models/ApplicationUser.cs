using Microsoft.AspNetCore.Identity;

namespace ToDoListAppA2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string TypeOfUser { get; set; } = "Normal";
    }

}


