using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Stores
{
    public class DetailsModel : PageModel
    {
        private readonly DefaultDbContext _context;

        public DetailsModel(DefaultDbContext context)
        {
            _context = context;
        }

        public Store? Store { get; set; }

        public async Task OnGetAsync(Guid? storeid = null)
        {
            if (storeid == null)
            {
                Store = null;
                return;
            }

            Store = await _context.Stores
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == storeid.Value);
        }
    }
}
