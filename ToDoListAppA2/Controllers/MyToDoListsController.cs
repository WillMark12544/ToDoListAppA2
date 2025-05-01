using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using ToDoListAppA2.Data;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.Controllers
{
    public class MyToDoListsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Inject ApplicationDbContent and UserManager into controller
        public MyToDoListsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ToDoLists
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ToDoLists
                .Include(t => t.User)
                .Include(t => t.SharedWith)
                .ThenInclude(ts => ts.SharedWithUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ToDoLists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ToDoLists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] ToDoList toDoList)
        {
            if (ModelState.IsValid)
            {
                toDoList.UserId = _userManager.GetUserId(User);
                _context.Add(toDoList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(toDoList);
        }

        // GET: ToDoLists/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _context.ToDoLists.FindAsync(id);
            if (toDoList == null)
            {
                return NotFound();
            }
            return View(toDoList);
        }

        // POST: ToDoLists/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Description")] ToDoList toDoList)
        {
            if (!ModelState.IsValid)
            {
                return View(toDoList);
            }

            var existingToDoList = await _context.ToDoLists.FindAsync(id);
            if (existingToDoList == null)
            {
                return NotFound();
            }

            existingToDoList.UserId = _userManager.GetUserId(User);
            existingToDoList.Title = toDoList.Title;
            existingToDoList.Description = toDoList.Description;

            try
            {
                _context.Update(existingToDoList);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ToDoListExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ToDoLists/Delete
        public async Task<IActionResult> Delete(int? id)
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

        // POST: ToDoLists/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var toDoList = await _context.ToDoLists.FindAsync(id);
            if (toDoList != null)
            {
                _context.ToDoLists.Remove(toDoList);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ToDoList/Share
        public async Task<IActionResult> Share(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var toDoList = await _context.ToDoLists
                .FirstOrDefaultAsync(t => t.Id == id);

            if (toDoList == null)
            {
                return NotFound();
            }

            return View(toDoList);
        }

        // POST: ToDoList/Share
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(int id, string email)
        {
            var toDoList = await _context.ToDoLists.FirstOrDefaultAsync(t => t.Id == id);
            if (toDoList == null)
            {
                return NotFound();
            }

            // Check if email is null
            if (string.IsNullOrEmpty(email))
            {
                // Error messages for user feedback, sent to <span> in view
                ModelState.AddModelError("Email", "The Email field is required.");
                return View(toDoList);
            }

            // Check if user with email exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Could not find a User with that Email.");
                return View(toDoList);
            }

            // Check if the To-Do list has already been shared with an inputted email
            var isAlreadySharedWithUser = await _context.ToDoListShares
               .AnyAsync(s => s.ToDoListId == id && s.SharedWithUserId == user.Id);

            if (isAlreadySharedWithUser)
            {
                ModelState.AddModelError("Email", "This To-Do List is already shared with that User.");
                return View(toDoList);
            }

            // If checks pass, create a new ToDoListShare object
            var toDoListShare = new ToDoListShare
            {
                ToDoListId = id,
                SharedWithUserId = user.Id
            };

            // Add new ToDoListShare to database
            _context.ToDoListShares.Add(toDoListShare);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        private bool ToDoListExists(int id)
        {
            return _context.ToDoLists.Any(e => e.Id == id);
        }
    }
}
