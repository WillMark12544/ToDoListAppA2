using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListAppA2.Data;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.Controllers
{
    public class ToDoListNodesController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Inject ApplicationDbContent and UserManager into controller
        public ToDoListNodesController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ToDoLists/Details/5
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _context.ToDoLists
                .Include(t => t.User)
                .Include(t => t.ToDoListNodes)
                .Include(t => t.SharedWith)
                .ThenInclude(s => s.SharedWithUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // GET: ToDoListNodes/Create
        public IActionResult Create(int toDoListId)
        {
            var model = new ToDoListNode
            {
                ToDoListId = toDoListId
            };

            return View(model);
        }

        // POST: ToDoListNodes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Status,DueDate,ToDoListId")] ToDoListNode toDoListNode)
        {
            if (ModelState.IsValid)
            {
                _context.ToDoListNodes.Add(toDoListNode);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", new { id = toDoListNode.ToDoListId});
            }
            return View(toDoListNode);
        }
    }
}
