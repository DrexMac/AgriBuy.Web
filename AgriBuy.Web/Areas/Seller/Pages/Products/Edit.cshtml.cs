using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
        public Product Product { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public string? StoreName { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid itemid)
        {
            var product = await _context.Products
                .Include(p => p.Store)
                .FirstOrDefaultAsync(p => p.Id == itemid);

            if (product == null)
                return NotFound();

            Product = product;
            StoreName = product.Store?.Name;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existing = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Product.Id);
            if (existing == null)
                return NotFound();

            // Preserve StoreId and UserId
            Product.StoreId = existing.StoreId;
            Product.UserId = existing.UserId;

            // Handle image update if a new file is uploaded
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "products");
                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(ImageFile.FileName);
                var filename = Product.Id + extension;
                var filePath = Path.Combine(uploadsFolder, filename);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await ImageFile.CopyToAsync(stream);

                Product.ImagePath = "/products/" + filename;
            }
            else
            {
                Product.ImagePath = existing.ImagePath; // keep old image
            }

            _context.Products.Update(Product);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Stores/Details", new { area = "Seller", storeid = Product.StoreId });
        }
    }
}
