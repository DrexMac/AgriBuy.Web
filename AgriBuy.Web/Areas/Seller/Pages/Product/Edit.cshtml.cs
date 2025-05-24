using AgriBuy.Contracts.Dto;
using AgriBuy.Models.ViewModels;
using AgriBuy.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Product
{
    public class EditModel : PageModel
    {
        private readonly ProductService _productService;
        private readonly StoreService _storeService;
        private readonly IMapper _mapper;

        public EditModel(ProductService productService, StoreService storeService, IMapper mapper)
        {
            _productService = productService;
            _storeService = storeService;
            _mapper = mapper;
        }

        [BindProperty]
        public ProductDto Product { get; set; } = new();

        [BindProperty]
        public Guid SelectedStoreId { get; set; }

        public List<SelectListItem> StoreOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var productDto = await _productService.GetByIdAsync(id);
            if (productDto == null)
                return NotFound();

            Product = productDto;
            SelectedStoreId = productDto.StoreId;

            var stores = await _storeService.GetAllAsync();
            StoreOptions = stores.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            Product.StoreId = SelectedStoreId;
            await _productService.UpdateAsync(Product);

            return RedirectToPage("Index");
        }
    }
}
