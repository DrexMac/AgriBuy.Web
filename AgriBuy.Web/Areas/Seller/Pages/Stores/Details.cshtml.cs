using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AgriBuy.Web.Areas.Seller.Pages.Stores
{
    public class DetailsModel : PageModel
    {
        private readonly DefaultDbContext _context;

        public DetailsModel(DefaultDbContext context)
        {
            _context = context;
        }

        public Store? Store { get; set; }
        public IList<Product> Products { get; set; } = new List<Product>();

        public async Task<IActionResult> OnGetAsync(Guid? storeid = null)
        {
            if (storeid == null)
            {
                return NotFound();
            }

            Store = await _context.Stores
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == storeid.Value);

            if (Store == null)
            {
                return NotFound();
            }

            Products = Store.Products?.ToList() ?? new List<Product>();

            return Page();
        }
    }
}
