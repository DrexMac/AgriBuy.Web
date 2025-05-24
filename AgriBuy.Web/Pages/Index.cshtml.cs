using AgriBuy.Contracts;
using AgriBuy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace AgriBuy.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public LoginVm LoginVm { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Login", new { area = "Accounts" });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userService.GetByEmailAsync(LoginVm.EmailAddress);
            if (user == null || !await _userService.CheckPasswordAsync(LoginVm.EmailAddress, LoginVm.Password))
            {
                ErrorMessage = "Invalid credentials.";
                return Page();
            }

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserRole", user.Role.ToString());

            return RedirectToPage("/Home"); // or your dashboard
        }
    }
}
