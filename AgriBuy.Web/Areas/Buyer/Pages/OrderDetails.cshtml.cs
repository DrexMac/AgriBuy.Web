using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
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

        public async Task OnGetAsync(Guid? userId = null, bool latestOnly = false)
        {
            if (userId == null)
                return;

            var query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId && o.IsPaid)
                .OrderByDescending(o => o.PayDate);

            if (latestOnly)
                Orders = await query.Take(1).ToListAsync();
            else
                Orders = await query.ToListAsync();
        }
    }
}
