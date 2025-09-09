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
            // Get current user
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out var userId))
                return RedirectToPage("/Accounts/Login");

            // 1 store = 1 user setup
            var store = await _context.Stores
                .Where(s => s.UserId == userId)
                .Include(s => s.Products)
                .FirstOrDefaultAsync();

            if (store != null)
            {
                // pag may store
                return RedirectToPage("/Stores/Details", new { storeid = store.Id });
            }

            // pag wala o na delete
            Stores = new List<Store>();
            return Page();
        }

    }
}
