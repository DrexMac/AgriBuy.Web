using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Accounts.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            // Sign out the user
            await HttpContext.SignOutAsync();

            // Redirect to home page after logout
            return RedirectToPage("/Index");
        }
    }
}
