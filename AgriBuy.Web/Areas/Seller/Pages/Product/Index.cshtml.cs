using AgriBuy.Contracts.Dto;
using AgriBuy.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Product
{
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;
        private readonly StoreService _storeService;

        public IndexModel(ProductService productService, StoreService storeService)
        {
            _productService = productService;
            _storeService = storeService;
        }

        public List<ProductListItemVm> Products { get; set; } = new();

        public async Task OnGetAsync()
        {
            var productDtos = await _productService.GetAllAsync();
            var storeDtos = await _storeService.GetAllAsync();

            Products = productDtos.Select(p => new ProductListItemVm
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                UnitPrice = p.UnitPrice,
                Quantity = p.Quantity,
                UnitOfMeasure = p.UnitOfMeasure,
                StoreName = storeDtos.FirstOrDefault(s => s.Id == p.StoreId)?.Name ?? "Unknown"
            }).ToList();
        }

        public class ProductListItemVm
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public string UnitOfMeasure { get; set; } = null!;
            public bool IsAvailable { get; set; }
            public string StoreName { get; set; } = null!;
        }
    }
}
