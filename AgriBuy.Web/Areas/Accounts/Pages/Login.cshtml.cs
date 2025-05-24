using AgriBuy.Contracts;
using AgriBuy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Accounts.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginVm Input { get; set; } = new LoginVm();

        public string? ErrorMessage { get; set; }
        private readonly IUserService _userService;

        public LoginModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
            // You could clear session here if you want to ensure a clean login
            // HttpContext.Session.Clear();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userDto = await _userService.GetByEmailAsync(Input.EmailAddress);
            if (userDto == null || !await _userService.CheckPasswordAsync(Input.EmailAddress, Input.Password))
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Store session values
            HttpContext.Session.SetString("UserId", userDto.Id.ToString());
            HttpContext.Session.SetString("UserRole", userDto.Role.ToString());

            // Redirect to landing page based on role
            return RedirectToPage("/Landing");
        }
    }
}
