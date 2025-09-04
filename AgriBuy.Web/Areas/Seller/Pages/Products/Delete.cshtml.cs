using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Products
{
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public Product? Item { get; set; }

        private readonly DefaultDbContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(DefaultDbContext context, ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync(Guid? itemid = null)
        {
            if (itemid == null)
            {
                Item = null;
                return;
            }

            Item = await _context.Products
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.Id == itemid.Value);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Item == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(Item.Id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}