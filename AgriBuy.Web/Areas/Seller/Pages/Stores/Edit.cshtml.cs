using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Stores
{
    public class EditModel : PageModel
    {
        private readonly DefaultDbContext _context;

        [BindProperty]
        public Store? Store { get; set; }

        public EditModel(DefaultDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(Guid? storeid = null)
        {
            if (storeid == null)
            {
                return NotFound();
            }

            Store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == storeid.Value);

            if (Store == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || Store == null)
            {
                return Page();
            }

            _context.Stores.Update(Store);
            await _context.SaveChangesAsync();

            // Redirect to the details of the updated store
            return RedirectToPage("/Stores/Details", new { area = "Seller", storeid = Store.Id });
        }
    }
}
