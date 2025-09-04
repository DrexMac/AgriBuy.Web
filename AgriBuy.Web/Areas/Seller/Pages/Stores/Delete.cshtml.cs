using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Stores
{
    public class DeleteModel : PageModel
    {
        [BindProperty]
        public Store? Item { get; set; }

        private readonly DefaultDbContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(DefaultDbContext context, ILogger<DeleteModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync(Guid? storeid = null)
        {
            if (storeid == null)
            {
                Item = null;
                return;
            }

            Item = await _context.Stores
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == storeid.Value);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Item == null)
            {
                return NotFound();
            }

            var store = await _context.Stores.FindAsync(Item.Id);
            if (store == null)
            {
                return NotFound();
            }

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}