using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
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

        public async Task OnGetAsync(Guid? storeid = null)
        {
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
    }
}
