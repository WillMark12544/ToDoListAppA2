using Microsoft.AspNetCore.Mvc;

namespace ToDoListAppA2.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "CartItem";

        public IActionResult AddToCart()
        {
            HttpContext.Session.SetString(CartSessionKey, "Premium");
            TempData["SuccessMessage"] = "Premium added to your cart.";

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Cart", "Shopping"); // fallback
        }


        public IActionResult ViewCart()
        {
            var item = HttpContext.Session.GetString(CartSessionKey);
            if (item == null)
            {
                ViewBag.Message = "Your cart is empty.";
            }
            else
            {
                ViewBag.Item = item;
            }
            return View();
        }

        public IActionResult ProceedToPayment()
        {
            var item = HttpContext.Session.GetString(CartSessionKey);
            if (item == null)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("ViewCart");
            }

            return RedirectToAction("Upgrade", "Payment");
        }

        public IActionResult RemoveFromCart()
        {
            HttpContext.Session.Remove("CartItem");
            TempData["SuccessMessage"] = "Item removed from cart.";
            return RedirectToAction("ViewCart");
        }

    }

}
