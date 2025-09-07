using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Accounts.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Clear the session values set during login
            HttpContext.Session.Clear();

            // Redirect to login page after logout
            return RedirectToPage("/Login", new { area = "Accounts" });
        }
    }
}
