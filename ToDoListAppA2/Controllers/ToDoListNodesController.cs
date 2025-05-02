using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
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

        // GET: ToDoListNodes/Index
        public async Task<IActionResult> Index(int? id, string source)
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

            // Use ViewBag to pass the navigation source around the controller
            ViewBag.Source = source;

            return View(toDoList);
        }

        // GET: ToDoListNodes/Create
        public IActionResult Create(int toDoListId, string source)
        {
            var model = new ToDoListNode
            {
                ToDoListId = toDoListId
            };

            ViewBag.Source = source;
            return View(model);
        }

        // POST: ToDoListNodes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Status,DueDate,ToDoListId")] ToDoListNode toDoListNode, string source)
        {
            if (ModelState.IsValid)
            {
                _context.ToDoListNodes.Add(toDoListNode);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = toDoListNode.ToDoListId, source = source});
            }

            ViewBag.Source = source;
            return View(toDoListNode);
        }

        // GET: ToDoListNodes/Edit
        public async Task<IActionResult> Edit(int id, string source)
        {
            var existingNode = await _context.ToDoListNodes.FindAsync(id);
            if (existingNode == null)
            {
                return NotFound();
            }

            ViewBag.Source = source;
            return View(existingNode);
        }

        // POST: ToDoListNodes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Description,Status,DueDate,ToDoListId")] ToDoListNode toDoListNode, string source)
        {
            if (!ModelState.IsValid)
            {
                return View(toDoListNode);
            }

            var existingToDoListNode = await _context.ToDoListNodes.FindAsync(id);
            if (existingToDoListNode == null)
            {
                return NotFound();
            }

            existingToDoListNode.Title = toDoListNode.Title;
            existingToDoListNode.Description = toDoListNode.Description;
            existingToDoListNode.Status = toDoListNode.Status;
            existingToDoListNode.DueDate = toDoListNode.DueDate;

            try
            {
                _context.Update(existingToDoListNode);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToDoListNodeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            ViewBag.Source = source;
            return RedirectToAction(nameof(Index), new { id = toDoListNode.ToDoListId, source = source});
        }

        // GET: ToDoListNode/Delete
        public async Task<IActionResult> Delete(int? id, string source)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoListNode = await _context.ToDoListNodes
                .FirstOrDefaultAsync(tn => tn.Id == id);
            if (toDoListNode == null)
            {
                return NotFound();
            }

            ViewBag.Source = source;
            return View(toDoListNode);
        }

        // POST: ToDoListNode/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string source)
        {
            var toDoListNode = await _context.ToDoListNodes.FindAsync(id);
            if (toDoListNode != null)
            {
                _context.ToDoListNodes.Remove(toDoListNode);
            }

            await _context.SaveChangesAsync();
            ViewBag.Source = source;
            return RedirectToAction(nameof(Index), new { id = toDoListNode.ToDoListId, source = source});
        }

        private bool ToDoListNodeExists(int id)
        {
            return _context.ToDoListNodes.Any(tn => tn.Id == id);
        }
    }
}
