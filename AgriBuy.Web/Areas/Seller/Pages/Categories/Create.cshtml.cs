using AgriBuy.Contracts;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public CreateModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Category Input { get; set; } = new();

        public IEnumerable<Category> ExistingCategories { get; set; } = new List<Category>();

        public async Task OnGetAsync()
        {
            ExistingCategories = await _categoryService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            await _categoryService.AddAsync(Input);
            return RedirectToPage("Index");
        }
    }
}
