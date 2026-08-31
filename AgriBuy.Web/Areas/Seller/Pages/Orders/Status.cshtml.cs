using AgriBuy.EntityFramework;
using AgriBuy.Models.Enums;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Orders
{
    public class StatusModel : PageModel
    {
        private readonly DefaultDbContext _context;

        public StatusModel(DefaultDbContext context)
        {
            _context = context;
        }

        public List<Order> Orders { get; set; } = new();

        // Store ID used for the Back to Store button
        public Guid? StoreId { get; set; }

        public async Task OnGetAsync(Guid? storeid = null)
        {
            // Keep the store ID so the page can navigate
            // back to the correct store.
            StoreId = storeid;

            if (storeid == null)
                return;

            Orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o =>
                    o.StoreId == storeid &&
                    o.IsPaid)
                .OrderByDescending(o => o.PayDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(
            Guid orderId,
            OrderStatus newStatus,
            Guid storeid)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.StoreId == storeid);

            if (order == null)
            {
                return RedirectToPage(new { storeid });
            }

            // Update order status
            order.Status = newStatus;

            // Set fulfillment date when delivered
            if (newStatus == OrderStatus.Delivered)
            {
                order.FulfillmentDate = DateTime.UtcNow;
            }
            else
            {
                order.FulfillmentDate = null;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { storeid });
        }
    }
}
