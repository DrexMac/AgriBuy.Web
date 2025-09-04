using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
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

        public async Task OnGetAsync()
        {
            Products = (IEnumerable<ProductDto>)await _productService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(Guid productId, int quantity)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Accounts/Login");
            }

            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var itemPrice = product.UnitPrice * quantity;

            var cartItem = new ShoppingCartDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProductId = productId,
                UnitPrice = product.UnitPrice,
                Quantity = quantity,
                ItemPrice = itemPrice,
                UnitOfMeasure = product.UnitOfMeasure
            };

            await _shoppingCartService.AddAsync(cartItem);
            return RedirectToPage("/Buyer/ShoppingCart"); 
        }
    }
}
