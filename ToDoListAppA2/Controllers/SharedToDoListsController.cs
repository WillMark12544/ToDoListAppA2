using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListAppA2.Data;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.Controllers
{
    public class SharedToDoListsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SharedToDoListsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ToDoListsShares
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var sharedToDoLists = await _context.ToDoListShares
                .Where(ts => ts.SharedWithUserId == userId)
                .Include(ts => ts.ToDoList)
                .Select(ts => ts.ToDoList)
                .ToListAsync();

            return View(sharedToDoLists);
        }
    }
}