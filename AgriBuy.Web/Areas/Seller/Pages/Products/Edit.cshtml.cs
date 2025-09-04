using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgriBuy.Web.Areas.Seller.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly DefaultDbContext _context;

        public EditModel(DefaultDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Product Products { get; set; }

        public string? StoreName { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid itemid)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.Id == itemid);

            if (product == null)
            {
                return NotFound();
            }
            Products = product;
            StoreName = product.Store?.Name;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Ensure StoreId is preserved
            var existing = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Products.Id);
            if (existing != null)
            {
                Products.StoreId = existing.StoreId;
            }

            _context.Products.Update(Products);
            await _context.SaveChangesAsync();
            return RedirectToPage(nameof(Index));
        }
    }
}