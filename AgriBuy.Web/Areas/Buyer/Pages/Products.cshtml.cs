using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Buyer.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;

        public ProductsModel(IProductService productService, IShoppingCartService shoppingCartService)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
        }

        public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();

        [BindProperty]
        public AddToCartVm AddToCartInput { get; set; } = new AddToCartVm();

        public class AddToCartVm
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        public async Task OnGetAsync()
        {
            Products = await _productService.GetAllProductsAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Accounts/Login");

            var product = await _productService.GetByIdAsync(AddToCartInput.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("", "Product not found.");
                await OnGetAsync();
                return Page();
            }

            var cartItem = new ShoppingCartDto
            {
                UserId = userId,
                ProductId = product.Id,
                Quantity = AddToCartInput.Quantity,
                UnitPrice = product.UnitPrice,
                UnitOfMeasure = product.UnitOfMeasure,
                ProductName = product.Name,
                ImagePath = product.ImagePath,
                ItemPrice = product.UnitPrice * AddToCartInput.Quantity
            };

            await _shoppingCartService.AddAsync(cartItem);
            return RedirectToPage(); // Refresh page
        }
    }
}
