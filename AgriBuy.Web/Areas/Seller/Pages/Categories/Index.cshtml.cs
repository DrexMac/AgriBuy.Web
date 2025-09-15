using AgriBuy.Contracts;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();

        public IndexModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetAllAsync();
        }
    }
}
