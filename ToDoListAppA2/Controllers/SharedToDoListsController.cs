using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Runtime.CompilerServices;
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
                // To display shared users on Index page
                    .ThenInclude(t => t.SharedWith)
                        .ThenInclude(t => t.SharedWithUser)
                .Select(ts => ts.ToDoList)
                .ToListAsync();

            return View(sharedToDoLists);
        }

        // GET: ToDoListShares/Leave
        public async Task<IActionResult> Leave(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _context.ToDoLists
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // POST: ToDoListShares/Leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = _userManager.GetUserId(User);

            var toDoListSharedWithUser = await _context.ToDoListShares
                .FirstOrDefaultAsync(ts => ts.ToDoListId == id && ts.SharedWithUserId == userId);

            if (toDoListSharedWithUser != null)
            {
                _context.ToDoListShares.Remove(toDoListSharedWithUser);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


    }
}