using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Accounts/Login");

            Orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId && o.IsPaid)
                .OrderByDescending(o => o.PayDate)
                .ToListAsync();

            // ---- NEW: mark all as seen so the "Updated!" badge clears ----
            var unseen = Orders.Where(o => !o.IsSeenByBuyer).ToList();
            if (unseen.Any())
            {
                foreach (var o in unseen)
                {
                    o.IsSeenByBuyer = true;
                }
                await _context.SaveChangesAsync();
            }

            return Page();
        }
    }
}