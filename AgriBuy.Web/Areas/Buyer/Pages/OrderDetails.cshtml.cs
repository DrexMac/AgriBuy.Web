using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgriBuy.Web.Areas.Buyer.Pages.Cart
{
    public class OrderDetailsModel : PageModel
    {
        private readonly DefaultDbContext _context;

        public OrderDetailsModel(DefaultDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the logged-in buyer.
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Accounts/Login");
            }

            // Load paid orders belonging to this buyer.
            // DeliveryImagePath is a string property on Order,
            // so it does not require Include().
            Orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o =>
                    o.UserId == userId &&
                    o.IsPaid)
                .OrderByDescending(o => o.PayDate)
                .ToListAsync();

            return Page();
        }
    }
}
