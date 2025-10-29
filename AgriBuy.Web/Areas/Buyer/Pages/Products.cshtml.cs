using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using Microsoft.AspNetCore.Http;
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
        public List<AgriBuy.Models.Models.Category> AllCategories { get; set; } = new();
        public Guid? SelectedCategoryId { get; set; }

        public async Task OnGetAsync(Guid? categoryId = null)
        {
            var allProducts = await _productService.GetAllAsync();
            var allCategories = await _categoryService.GetRootCategoriesAsync();

            AllCategories = allCategories.ToList();
            SelectedCategoryId = categoryId;

            if (categoryId.HasValue)
            {
                CategoriesWithProducts = allCategories
                    .Where(c => c.Id == categoryId.Value)
                    .Select(c => new CategoryWithProductsVm
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Products = allProducts
                            .Where(p => p.CategoryId == c.Id)
                            .ToList()
                    })
                    .ToList();
            }
            else
            {
                CategoriesWithProducts = allCategories
                    .Select(c => new CategoryWithProductsVm
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Products = allProducts
                            .Where(p => p.CategoryId == c.Id)
                            .ToList()
                    })
                    .ToList();
            }
        }

        public async Task<IActionResult> OnPostAddToCartAsync(Guid productId, int quantity)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return RedirectToPage("/Accounts/Login", new { area = "Buyer" });

            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
            {
                ModelState.AddModelError("", "Product not found.");
                await OnGetAsync();
                return Page();
            }

            if (product.Quantity < quantity)
            {
                ModelState.AddModelError("", $"Not enough stock for {product.Name}. Available: {product.Quantity}");
                await OnGetAsync();
                return Page();
            }

            var existingItems = await _shoppingCartService.GetByUserIdAsync(userId);
            var existing = existingItems.FirstOrDefault(ci => ci.ProductId == product.Id);

            if (existing != null)
            {
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
