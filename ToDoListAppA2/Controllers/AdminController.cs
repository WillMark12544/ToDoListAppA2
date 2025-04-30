using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchTerm, string roleFilter)
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
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "Invalid user or role.");
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return RedirectToAction("Index");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var existingRole = currentRoles.FirstOrDefault(); // Finds first role 

            if (!string.IsNullOrEmpty(existingRole))
            {
                //Removes users current role
                await _userManager.RemoveFromRoleAsync(user, existingRole);
            }

            //Role exists
            if (await _roleManager.RoleExistsAsync(role))
            {
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
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Role does not exist.");
                return RedirectToAction("Index");
            }

            // On success, redirect to the index page
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId) //Deletes UserIDS, Need to Add ToDoLists, Nodes and Shares later !!!!!!
        {
            if (string.IsNullOrEmpty(userId)) // Checks user exists
            {
                TempData["ErrorMessage"] = "Invalid user ID."; 
                return RedirectToAction("Index");
            }

            //Search for user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found."; 
                return RedirectToAction("Index");
            }

            //Stop an Admin for deleting themself
            if (user.Email == User.Identity.Name)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account."; 
                return RedirectToAction("Index");
            }

            //Actualy deletes the user
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



    }
}
