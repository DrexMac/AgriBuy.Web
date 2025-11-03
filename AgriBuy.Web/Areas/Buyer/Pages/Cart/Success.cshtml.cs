using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Buyer.Pages.Cart
{
    public class SuccessModel : PageModel
    {
        private readonly DefaultDbContext _context;

        public SuccessModel(DefaultDbContext context)
        {
            _context = context;
        }

        public Order? LatestPaidOrder { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Accounts/Login");

            // Find latest unpaid order (just placed)
            var latestOrder = await _context.Orders
                .Where(o => o.UserId == userId && !o.IsPaid)
                .OrderByDescending(o => o.OrderDate)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (latestOrder != null)
            {
                // Mark as paid
                latestOrder.IsPaid = true;
                latestOrder.PayDate = DateTime.UtcNow;
                latestOrder.ORNumber = $"OR-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";

                // Deduct product stock
                foreach (var item in latestOrder.OrderItems)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.Quantity -= item.Quantity;
                        _context.Products.Update(product);
                    }
                }

                _context.Orders.Update(latestOrder);
                await _context.SaveChangesAsync();
            }

            // Clear cart after success
            var cartItems = await _context.ShoppingCarts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _context.ShoppingCarts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }

            // Load the latest *paid* order (to display summary)
            LatestPaidOrder = await _context.Orders
                .Where(o => o.UserId == userId && o.IsPaid)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.PayDate)
                .FirstOrDefaultAsync();

            return Page();
        }
    }
}
