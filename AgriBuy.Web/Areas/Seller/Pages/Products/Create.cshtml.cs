using AgriBuy.Contracts;
using AgriBuy.Models.Models;
using AgriBuy.Models.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        [BindProperty]
        public ProductViewModel Input { get; set; } = new();

        [BindProperty]
        public Guid? StoreId { get; set; }

        public CreateModel(
            IStoreService storeService,
            IProductService productService,
            IMapper mapper)
        {
            _storeService = storeService;
            _productService = productService;
            _mapper = mapper;
        }

        public IActionResult OnGet(Guid? storeId = null)
        {
            StoreId = storeId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Get current user
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Accounts/Login");

            // Ensure store exists
            Store store = null;
            if (StoreId.HasValue)
            {
                store = await _storeService.GetByIdAsync(StoreId.Value);
            }

            if (store == null)
            {
                store = new Store
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "My Store"
                };
                await _storeService.AddAsync(store);
            }

            // Map Product from Input
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = Input.Name,
                UnitOfMeasure = Input.UnitOfMeasure,
                UnitPrice = Input.UnitPrice,
                Quantity = Input.Quantity,
                Description = Input.Description,
                StoreId = store.Id,
                UserId = userId
            };

            // Handle image upload
            if (Input.Image != null && Input.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "products");
                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(Input.Image.FileName);
                var filename = product.Id + extension;
                var filePath = Path.Combine(uploadsFolder, filename);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await Input.Image.CopyToAsync(stream);

                product.ImagePath = "/products/" + filename;
            }

            // Save product
            await _productService.AddAsync(product);

            return RedirectToPage("/Stores/Details", new { area = "Seller", storeid = store.Id });
        }
    }
}
