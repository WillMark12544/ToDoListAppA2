﻿using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> ChangeRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            // Prevent current user from changing their own role
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["ErrorMessage"] = "You cannot change your own role.";
                return RedirectToAction("Index");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var existingRole = currentRoles.FirstOrDefault();

            // Skip if role is the same
            if (existingRole == role)
            {
                TempData["InfoMessage"] = "No changes were made. User is already in the selected role.";
                return RedirectToAction("Index");
            }

            // Remove current role if it exists
            if (!string.IsNullOrEmpty(existingRole))
            {
                await _userManager.RemoveFromRoleAsync(user, existingRole);
            }

            // Add the new role
            var addRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["ErrorMessage"] = "Changing user role unsuccessful.";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "User's role changed successfully.";
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
                TempData["SuccessMessage"] = "User deleted successfully.";
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
            TempData["SuccessMessage"] = "To-Do List updated successfully!";
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

            TempData["SuccessMessage"] = "To-Do List deleted successfully!";
            return RedirectToAction("ViewUserToDoLists", new { userId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageShares(int listId)
        {
            var list = await _context.ToDoLists
                .Include(t => t.SharedWith)
                .ThenInclude(s => s.SharedWithUser)
                .FirstOrDefaultAsync(t => t.Id == listId);

            if (list == null)
                return NotFound();

            ViewBag.ListId = listId;
            ViewBag.ListTitle = list.Title;
            return View(list.SharedWith.ToList());
        }

        // Add a user to shared list by email
        [HttpPost]
        public async Task<IActionResult> AddSharedUser(int listId, string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail)) // Checks Null
            {
                TempData["ErrorMessage"] = "Email cannot be empty.";
                return RedirectToAction("ManageShares", new { listId });
            }

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ManageShares", new { listId });
            }

            var list = await _context.ToDoLists.FindAsync(listId); //Stops sharing with owner

            if (list.UserId == user.Id)
            {
                TempData["ErrorMessage"] = "You cannot share a list with its owner.";
                return RedirectToAction("ManageShares", new { listId });
            }

            //stops duplicate shares
            bool alreadyShared = await _context.ToDoListShares.AnyAsync(s =>
                s.ToDoListId == listId && s.SharedWithUserId == user.Id);

            if (alreadyShared)
            {
                TempData["ErrorMessage"] = "This user is already shared on the list.";
                return RedirectToAction("ManageShares", new { listId });
            }

            var newShare = new ToDoListShare
            {
                ToDoListId = listId,
                SharedWithUserId = user.Id
            };

            _context.ToDoListShares.Add(newShare);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User added to shared list.";
            return RedirectToAction("ManageShares", new { listId });
        }

        //Remove User - SharedLists
        [HttpPost]
        public async Task<IActionResult> RemoveSharedUser(int shareId, int listId)
        {
            var share = await _context.ToDoListShares.FindAsync(shareId);
            if (share == null)
            {
                TempData["ErrorMessage"] = "Share not found.";
                return RedirectToAction("ManageShares", new { listId });
            }

            _context.ToDoListShares.Remove(share);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User removed from shared list.";
            return RedirectToAction("ManageShares", new { listId });
        }
    }
}
