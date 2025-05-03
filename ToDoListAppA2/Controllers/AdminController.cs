using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListAppA2.Data;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.Controllers
{
    [Authorize(Roles = "Admin")] //Restricts Access to Admins Only
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;


        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager; 
            _roleManager = roleManager; 
            _context = context;
        }

        public async Task<IActionResult> Index(string searchTerm, string roleFilter) //Search Bar
        {
            var users = _userManager.Users.ToList(); //Loads users from DB
            var userViewModels = new List<(ApplicationUser User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add((user, roles));
            }

            //Email Saerhc
            if (!string.IsNullOrEmpty(searchTerm))
            {
                userViewModels = userViewModels
                    .Where(u => u.User.Email != null && u.User.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            //Roles Search
            if (!string.IsNullOrEmpty(roleFilter))
            {
                userViewModels = userViewModels
                    .Where(u => u.Roles.Contains(roleFilter, StringComparer.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewData["SearchTerm"] = searchTerm;
            ViewData["RoleFilter"] = roleFilter;

            return View(userViewModels);
        }




        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string role) //Change role function
        {

            var user = await _userManager.FindByIdAsync(userId); //Finds User

            var currentRoles = await _userManager.GetRolesAsync(user);
            var existingRole = currentRoles.FirstOrDefault(); 

            if (!string.IsNullOrEmpty(existingRole))
            {
                //Removes users current role
                await _userManager.RemoveFromRoleAsync(user, existingRole);
            }

            //Adds RoleSs
            var addRoleResult = await _userManager.AddToRoleAsync(user, role);

            if (!addRoleResult.Succeeded)
            {
                //Redirect if failure
                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return RedirectToAction("Index");
            }

            // On success, redirect to the index page
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            if (user.Email == User.Identity.Name)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction("Index");
            }

            var userLists = _context.ToDoLists // Gathers ToDoLists
                .Where(t => t.UserId == userId)
                .ToList();

            foreach (var list in userLists) //Deletes nodes and Shares
            {
                _context.ToDoLists.Remove(list);
            }

            // Deletes assosiation to list shared with them
            var sharedWithUser = _context.ToDoListShares
                .Where(s => s.SharedWithUserId == userId)
                .ToList();

            _context.ToDoListShares.RemoveRange(sharedWithUser);

            // Saves before deleting user
            await _context.SaveChangesAsync();

            //Deletes User
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User and associated data deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user.";
            }

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> ViewUserToDoLists(string userId) //View users ToDoLists
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            var todoLists = await _context.ToDoLists
                .Where(t => t.UserId == userId)
                .ToListAsync();

            ViewBag.UserEmail = user.Email;

            return View(todoLists); 
        }

        //Admin CRUD Operations On User Notes

        [HttpGet]
        public async Task<IActionResult> EditToDoList(int id) //Edit 
        {
            var list = await _context.ToDoLists.FindAsync(id);
            if (list == null)
                return NotFound();

            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> EditToDoList(ToDoList updatedList)
        {
            if (!ModelState.IsValid)
                return View(updatedList);

            var existing = await _context.ToDoLists.FindAsync(updatedList.Id);
            if (existing == null)
                return NotFound();

            existing.Title = updatedList.Title;
            existing.Description = updatedList.Description;

            await _context.SaveChangesAsync();
            return RedirectToAction("ViewUserToDoLists", new { userId = existing.UserId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteToDoList(int id) //Delete
        {
            var list = await _context.ToDoLists.FindAsync(id);
            if (list == null)
                return NotFound();

            var userId = list.UserId;

            _context.ToDoLists.Remove(list);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewUserToDoLists", new { userId });
        }





    }
}
