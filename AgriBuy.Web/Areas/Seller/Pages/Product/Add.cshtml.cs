using AgriBuy.Models.ViewModels;
using AgriBuy.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgriBuy.Contracts.Dto;
using System.Security.Claims;

namespace AgriBuy.Web.Areas.Seller.Pages.Product
{
    public class AddModel : PageModel
    {
        private readonly ProductService _productService;
        private readonly StoreService _storeService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AddModel(ProductService productService, StoreService storeService, IHttpContextAccessor httpContextAccessor)
        {
            _productService = productService;
            _storeService = storeService;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public ProductViewModel ProductVM { get; set; } = new ProductViewModel
        {
            Stores = new List<string>(),
            StoreIds = new List<Guid>()
        };

        public List<SelectListItem> StoreList { get; set; } = new();

        [TempData]
        public required string Message { get; set; }

        public async Task OnGetAsync()
        {
            await LoadStoresAsync();
        }

        private async Task LoadStoresAsync()
        {
            var stores = await _storeService.GetAllAsync();
            StoreList = stores.Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            }).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadStoresAsync();
                return Page();
            }

            
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var currentUserId))
            {
                return Unauthorized(); // or RedirectToPage("/Account/Login")
            }

            var productDto = new ProductDto
            {
                Name = ProductVM.Name,
                Description = ProductVM.Description,
                UnitOfMeasure = ProductVM.UnitOfMeasure,
                UnitPrice = ProductVM.UnitPrice,
                Quantity = ProductVM.Quantity,
                StoreId = ProductVM.StoreId
            };

            await _productService.AddAsync(productDto, currentUserId); 

            Message = "Product added successfully!";
            return RedirectToPage("./Index");
        }
    }
}
