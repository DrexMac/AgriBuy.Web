using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using AgriBuy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Buyer.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IShoppingCartService _shoppingCartService;

        public ProductsModel(
            IProductService productService,
            ICategoryService categoryService,
            IShoppingCartService shoppingCartService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _shoppingCartService = shoppingCartService;
        }

        public List<CategoryWithProductsVm> CategoriesWithProducts { get; set; } = new();

        //  Load products and categories
        public async Task OnGetAsync(Guid? categoryId = null)
        {
            var allProducts = await _productService.GetAllAsync();
            var rootCategories = await _categoryService.GetRootCategoriesAsync();

            if (categoryId.HasValue)
            {
                var selectedCategory = rootCategories.FirstOrDefault(c => c.Id == categoryId.Value);
                if (selectedCategory != null)
                {
                    CategoriesWithProducts = new List<CategoryWithProductsVm>
                    {
                        new CategoryWithProductsVm
                        {
                            Id = selectedCategory.Id,
                            Name = selectedCategory.Name,
                            Products = allProducts.Where(p => p.CategoryId == selectedCategory.Id).ToList()
                        }
                    };
                }
            }
            else
            {
                CategoriesWithProducts = rootCategories
                    .Select(c => new CategoryWithProductsVm
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Products = allProducts.Where(p => p.CategoryId == c.Id).ToList()
                    })
                    .ToList();
            }
        }

        //  Add to Cart handler
        public async Task<IActionResult> OnPostAddToCartAsync(Guid productId, int quantity)
        {
            //  Resolve current user ID from session
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                // User not logged in ? redirect to login page
                return RedirectToPage("/Accounts/Login", new { area = "Buyer" });
            }

            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
            {
                ModelState.AddModelError("", "Product not found.");
                await OnGetAsync(); // reload products
                return Page();
            }

            //  Check stock
            if (product.Quantity < quantity)
            {
                ModelState.AddModelError("", $"Not enough stock for {product.Name}. Available: {product.Quantity}");
                await OnGetAsync();
                return Page();
            }

            //  Check if item exists in cart
            var existingItems = await _shoppingCartService.GetByUserIdAsync(userId);
            var existing = existingItems.FirstOrDefault(ci => ci.ProductId == product.Id);

            if (existing != null)
            {
                // Merge quantity
                existing.Quantity += quantity;
                existing.ItemPrice = existing.Quantity * existing.UnitPrice;
                await _shoppingCartService.UpdateAsync(existing);
            }
            else
            {
                var cartItem = new ShoppingCartDto
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
                await _shoppingCartService.AddAsync(cartItem);
            }

            //  Reduce product stock
            product.Quantity -= quantity;
            await _productService.UpdateAsync(product);

            //  Redirect to ShoppingCart page
            return RedirectToPage("/ShoppingCart", new { area = "Buyer" });
        }

        public class CategoryWithProductsVm
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public List<AgriBuy.Models.Models.Product> Products { get; set; } = new();
        }
    }
}
