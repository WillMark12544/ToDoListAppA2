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

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
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
    }
}
