using AgriBuy.Contracts;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Pages
{
    public class LandingModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;

        private readonly ICategoryService _categoryService;

        public LandingModel(
            IUserService userService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            ICategoryService categoryService)
        {
            _userService = userService;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _categoryService = categoryService;
        }

        public string? FullName { get; set; }
        public string? Role { get; set; }

        public IEnumerable<Product> FeaturedProducts { get; set; } = Enumerable.Empty<Product>();
        public IEnumerable<Category> Categories { get; set; } = Enumerable.Empty<Category>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Accounts/Login");
            }

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Accounts/Login");
            }

            FullName = $"{user.FirstName} {user.LastName}";
            Role = user.Role;

            // load products
            var allProducts = await _productService.GetAllAsync();
            FeaturedProducts = allProducts
                .OrderBy(p => p.Name)
                .Take(12)
                .ToList();

            // load categories 
            Categories = await _categoryService.GetRootCategoriesAsync();

            return Page();
        }

        // POST: /Landing?handler=AddToCart
        public async Task<IActionResult> OnPostAddToCartAsync(Guid productId, int quantity)
        {
            if (quantity < 1) quantity = 1;

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

            var existingItems = await _shoppingCartService.GetByUserIdAsync(userId);
            var existing = existingItems.FirstOrDefault(ci => ci.ProductId == productId);

            // Check stock for the requested quantity
            if (product.Quantity < quantity)
            {
                TempData["ErrorMessage"] = $"Not enough stock available for {product.Name}.";
                return RedirectToPage();
            }

            if (existing != null)
            {
                // Merge quantities in cart
                existing.Quantity += quantity;
                existing.ItemPrice = existing.Quantity * existing.UnitPrice;
                await _shoppingCartService.UpdateAsync(existing);

                // Reduce product stock every time
                product.Quantity -= quantity;
                await _productService.UpdateAsync(product);
            }
            else
            {
                // Add new cart item
                var cartItem = new AgriBuy.Contracts.Dto.ShoppingCartDto
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImagePath = product.ImagePath,
                    UnitPrice = product.UnitPrice,
                    Quantity = quantity,
                    UnitOfMeasure = product.UnitOfMeasure,
                    ItemPrice = product.UnitPrice * quantity
                };

                product.Quantity -= quantity; // Reduce stock
                await _shoppingCartService.AddAsync(cartItem);
                await _productService.UpdateAsync(product);
            }

            return RedirectToPage("/ShoppingCart", new { area = "Buyer" });
        }
    }
}
