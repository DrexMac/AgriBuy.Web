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

        [BindProperty(SupportsGet = true)]
        public Guid? StoreId { get; set; }

        public List<OrderStatus> AllStatuses { get; set; } =
            Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();

        public async Task OnGetAsync(Guid? storeid = null)
        {
            StoreId = storeid;

            if (storeid == null)
                return;

            Orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o => o.StoreId == storeid && o.IsPaid)
                .OrderByDescending(o => o.PayDate)
                .ToListAsync();
        }

        // ---- NEW: seller updates the order status ----
        public async Task<IActionResult> OnPostUpdateStatusAsync(Guid orderId, OrderStatus newStatus, Guid storeid)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = newStatus;

                // Mark as "not seen" so the buyer gets an "Updated!" badge
                order.IsSeenByBuyer = false;

                if (newStatus == OrderStatus.Delivered)
                {
                    order.FulfillmentDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { storeid });
        }
    }
}