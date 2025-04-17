using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using ToDoListAppA2.Models;
using Microsoft.AspNetCore.Authorization;

namespace ToDoListAppA2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                //Send users to login page
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            //Shows notes page if logged in
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
