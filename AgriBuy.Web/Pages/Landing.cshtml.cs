using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace AgriBuy.Web.Pages
{
    public class LandingModel : PageModel
    {
        private readonly IUserService _userService;

        public LandingModel(IUserService userService)
        {
            _userService = userService;
        }

        public string? FullName { get; set; }
        public string? Role { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Accounts/Login");
            }

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Accounts/Login");
            }

            FullName = $"{user.FirstName} {user.LastName}";
            Role = user.Role;

            return Page();
        }
    }
}
