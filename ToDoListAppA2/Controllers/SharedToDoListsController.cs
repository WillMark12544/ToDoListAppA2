using Microsoft.AspNetCore.Mvc;

namespace ToDoListAppA2.Controllers
{
    public class SharedToDoListsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
