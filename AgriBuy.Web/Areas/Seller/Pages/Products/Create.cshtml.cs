using AgriBuy.Contracts;
using AgriBuy.Models.Models;
using AgriBuy.Models.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgriBuy.Web.Areas.Seller.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        [BindProperty]
        public ProductViewModel Input { get; set; }

        public CreateModel(
            IStoreService storeService,
            IProductService productService,
            IOrderService orderService,
            IMapper mapper)
        {
            _storeService = storeService;
            _productService = productService;
            _orderService = orderService;
            _mapper = mapper;
        }

        public async Task<IActionResult> OnGet()
        {
            var orders = await _orderService.GetAllAsync();
            var ordersWithNumber = orders.Select(s => new Order
            {
                Id = s.Id,
                OrderNumber = string.IsNullOrEmpty(s.OrderNumber) ? "Unnamed Subject" : s.OrderNumber,
            }).ToList();

            ViewData["Orders"] = ordersWithNumber;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadOrdersAsync(); 
                return Page();
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Accounts/Login");
            }

            // Get store by user
            var store = await _storeService.GetAllAsync();
            var userStore = store.FirstOrDefault(s => s.UserId == userId);

            if (userStore == null)
            {
                ModelState.AddModelError(string.Empty, "You must create a store first.");
                await LoadOrdersAsync();
                return Page();
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),    
                Name = Input.Name,
                UnitOfMeasure = Input.UnitOfMeasure,
                UnitPrice = Input.UnitPrice,
                Quantity = Input.Quantity,
                Description = Input.Description,
                StoreId = userStore.Id
            };

            await _productService.AddAsync(product);

            if (Input.Image != null && Input.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "products");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var extension = Path.GetExtension(Input.Image.FileName);
                var filePath = Path.Combine(uploadsFolder, product.Id.ToString() + extension);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.Image.CopyToAsync(stream);
                }

                // (Optional) Save the relative image path in DB
                product.ImagePath = "/products/" + product.Id.ToString() + extension;
                await _productService.UpdateAsync(product);
            }

            return RedirectToPage("Index");
        }

        private async Task LoadOrdersAsync()
        {
            var orders = await _orderService.GetAllAsync();
            ViewData["Orders"] = orders;
        }
    }
}
