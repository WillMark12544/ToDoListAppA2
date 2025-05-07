using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ToDoListAppA2.Data;
using ToDoListAppA2.DataAccess.Repository.IRepository;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.Controllers
{
    public class ToDoListNodesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        // Inject ApplicationDbContent and UserManager into controller
        public ToDoListNodesController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: ToDoListNodes/Index
        public async Task<IActionResult> Index(int? id, string source)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id.Value);

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
                await _unitOfWork.toDoListNodes.AddAsync(toDoListNode);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index), new { id = toDoListNode.ToDoListId, source = source});
            }

            ViewBag.Source = source;
            return View(toDoListNode);
        }

        // GET: ToDoListNodes/Edit
        public async Task<IActionResult> Edit(int id, string source)
        {
            var existingNode = await _unitOfWork.toDoListNodes.GetByIdAsync(id);
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

            var existingNode = await _unitOfWork.toDoListNodes.GetByIdAsync(id);

            if (existingNode == null)
            {
                return NotFound();
            }

            existingNode.Title = toDoListNode.Title;
            existingNode.Description = toDoListNode.Description;
            existingNode.Status = toDoListNode.Status;
            existingNode.DueDate = toDoListNode.DueDate;

            try
            {
                await _unitOfWork.toDoListNodes.UpdateAsync(existingNode);
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _unitOfWork.toDoListNodes.ExistsAsync(id))
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

            var toDoListNode = await _unitOfWork.toDoListNodes.GetByIdAsync(id.Value);

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
            var toDoListNode = await _unitOfWork.toDoListNodes.GetByIdAsync(id);

            if (toDoListNode != null)
            {
                await _unitOfWork.toDoListNodes.DeleteAsync(toDoListNode);
                await _unitOfWork.SaveAsync();
            }

            ViewBag.Source = source;
            return RedirectToAction(nameof(Index), new { id = toDoListNode.ToDoListId, source = source});
        }
    }
}
