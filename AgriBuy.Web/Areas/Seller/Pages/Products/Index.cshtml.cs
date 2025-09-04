using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Mvc;

namespace AgriBuy.Web.Areas.Seller.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly DefaultDbContext _context;

        public IndexModel(DefaultDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new List<Product>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Accounts/Login");
            }

            var store = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);
            if (store != null)
            {
                Products = await _context.Products
                    .Include(p => p.Store)
                    .Where(p => p.StoreId == store.Id)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            else
            {
                Products = new List<Product>();
            }

            return Page();
        }
    }
}