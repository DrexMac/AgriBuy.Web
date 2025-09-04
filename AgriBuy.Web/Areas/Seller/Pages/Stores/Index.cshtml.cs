using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Stores
{
    public class IndexModel : PageModel
    {
        private readonly DefaultDbContext _context;

        public IndexModel(DefaultDbContext context)
        {
            _context = context;
        }

        public List<Store> Stores { get; set; } = new List<Store>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current logged-in user
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Accounts/Login");
            }

            // Get only stores that belong to this user
            Stores = await _context.Stores
                .Where(s => s.UserId == userId)
                .Include(s => s.Products) 
                .OrderBy(s => s.Name)
                .ToListAsync();

            return Page();
        }
    }
}
