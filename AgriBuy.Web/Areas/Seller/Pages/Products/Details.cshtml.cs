using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgriBuy.Web.Areas.Seller.Pages.Products
{
    public class DetailsModel : PageModel
    {
        private readonly DefaultDbContext _context;

        public DetailsModel(DefaultDbContext context)
        {
            _context = context;
        }

        public Product Product { get; set; }

        // Areas/Seller/Pages/Store/Create.cshtml.cs
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || _context.Products == null || Product == null)
            {
                return Page();
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Accounts/Login");
            }

            var store = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);
            if (store == null)
            {
                ModelState.AddModelError("", "Store not found for this user.");
                return Page();
            }

            Product.Id = Guid.NewGuid();
            Product.StoreId = store.Id; // <-- This is required!

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}