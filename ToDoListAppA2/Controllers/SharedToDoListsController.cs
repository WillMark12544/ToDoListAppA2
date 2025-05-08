using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SQLitePCL;
using System.Runtime.CompilerServices;
using ToDoListAppA2.Data;
using ToDoListAppA2.DataAccess.Repository.IRepository;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.Controllers
{
    public class SharedToDoListsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public SharedToDoListsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _context = context;
        }

        // GET: ToDoListsShares
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var sharedToDoLists = await _unitOfWork.sharedToDoLists.GetSharedToDoListsForUserAsync(userId);

            return View(sharedToDoLists);
        }

        // GET: ToDoListShares/Leave
        public async Task<IActionResult> Leave(int? id)
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

            return View(toDoList);
        }

        // POST: ToDoListShares/Leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var toDoListSharedWithUser = await _unitOfWork.sharedToDoLists.GetShareEntriesForToDoListAsync(id, currentUserId);

            if (toDoListSharedWithUser != null)
            {
                await _unitOfWork.sharedToDoLists.DeleteAsync(toDoListSharedWithUser);
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ToDoList/Share
        public async Task<IActionResult> Share(int? id)
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

            return View(toDoList);
        }

        // POST: ToDoList/Share
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(int id, string email)
        {
            // For displaying Title label in Share view
            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id);

            // Ensure To-Do List exists
            if (toDoList == null)
            {
                return NotFound();
            }

            // Check if email is null
            if (string.IsNullOrEmpty(email))
            {

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

            var currentUserEmail = User.Identity.Name;

            // Ensure user can't share a To-Do List with themselves
            // Compare entered email and current users email with case insensitive match
            if (string.Equals(email, currentUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                // Error messages for user feedback, sent to <span> in Share view
                ModelState.AddModelError("Email", "You can't share a To-Do List with yourself!");
                return View(toDoList);
            }

            // Check if the To-Do list has already been shared with an inputted email
            var isAlreadySharedWithUser = await _unitOfWork.myToDoLists.IsSharedWithUserAsync(id, user.Id);

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
            await _unitOfWork.sharedToDoLists.AddAsync(toDoListShare);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ToDoList/Unshare
        public async Task<IActionResult> Unshare(int? id)
        {
            if (id == null) return NotFound();

            var toDoList = await _context.ToDoLists
                .Include(t => t.SharedWith)
                .ThenInclude(s => s.SharedWithUser)
                .FirstOrDefaultAsync(t => t.Id == id.Value); 

            if (toDoList == null) return NotFound();

            return View(toDoList);
        }

        // POST: ToDoList/Unshare
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unshare(int id, string sharedWithUserId)
        {
            var toDoList = await _unitOfWork.myToDoLists.GetByIdAsync(id);

            // Ensure To-Do List exists
            if (toDoList == null)
            {
                return NotFound();
            }

            // Ensure User Id of user to remove is not null
            if (string.IsNullOrEmpty(sharedWithUserId))
            {
                ModelState.AddModelError("Email", "Please select a user to unshare the To-Do list from");
                return View(toDoList);
            }

            // Try to obtain the share entry pertaining to the selected ToDoList, and User to remove
            var userShareEntry = await _unitOfWork.sharedToDoLists.GetShareEntriesForToDoListAsync(id, sharedWithUserId);

            // Ensure the ToDoListShare entry exists
            if (userShareEntry == null)
            {
                ModelState.AddModelError("Email", "This To-Do list is not shared with that user!");
                return View(toDoList);
            }

            // If validation is successful, remove the share entry from the selected user
            await _unitOfWork.sharedToDoLists.DeleteAsync(userShareEntry);
            await _unitOfWork.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}