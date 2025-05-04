using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public PaymentController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        //GET
        [HttpGet]
        public IActionResult Upgrade()
        {
            ViewBag.PublishableKey = _config["Stripe:PublishableKey"];
            return View();
        }

        //POST
        [HttpPost]
        public async Task<IActionResult> Upgrade(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            //Check if Prem role is alr there
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Premium"))
            {
                TempData["ErrorMessage"] = "User is already a Premium member.";
                return RedirectToAction("Index");
            }

            //Removes Normal
            if (roles.Contains("Normal"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Normal");
            }

            //Adds Prem
            var result = await _userManager.AddToRoleAsync(user, "Premium");

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User successfully upgraded to Premium.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error upgrading to Premium.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession()
        {
            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "nzd",
                            UnitAmount = 999,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Premium Account Upgrade"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = domain + Url.Action("Success", "Payment"),
                CancelUrl = domain + Url.Action("Cancel", "Payment")
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success() //post Successful Payment
        {
            var user = await _userManager.GetUserAsync(User); // Gets User
            if (user == null)
                return Challenge();

            var currentRoles = await _userManager.GetRolesAsync(user); // Remove Roles
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            var addResult = await _userManager.AddToRoleAsync(user, "Premium"); //Adds PRem
            if (!addResult.Succeeded)
            {
                // Handle errors if adding the Premium role fails
                TempData["ErrorMessage"] = "Could not assign Premium role.";
                return View("PaymentCancel");
            }

            await _signInManager.RefreshSignInAsync(user); //Refreesh 

            return View("PaymentSuccess");
        }

        //Payment Failure
        public IActionResult Cancel()
        {
            return View("PaymentCancel");
        }
    }
}
