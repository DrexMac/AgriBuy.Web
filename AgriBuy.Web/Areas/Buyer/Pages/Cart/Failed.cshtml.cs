using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AgriBuy.Web.Areas.Buyer.Pages.Cart
{
    public class FailedModel : PageModel
    {
        private readonly ILogger<FailedModel> _logger;

        public FailedModel(ILogger<FailedModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Optionally log the payment failure
            _logger.LogWarning("Payment failed for a buyer checkout session.");
        }

        // Optional: if you want to handle redirect programmatically
        public IActionResult OnPostBackToCart()
        {
            // Redirect to the shopping cart page
            return RedirectToPage("/ShoppingCart", new { area = "Buyer" });
        }
    }
}