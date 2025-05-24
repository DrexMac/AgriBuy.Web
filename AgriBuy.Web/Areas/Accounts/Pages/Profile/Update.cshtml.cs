using System;
using System.Threading.Tasks;
using AgriBuy.Contracts.Dto;
using AgriBuy.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgriBuy.Web.Areas.Accounts.Pages.Profile
{
    public class UpdateModel : PageModel
    {
        private readonly IUserService _userService;

        public UpdateModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public UserDto Input { get; set; } = new UserDto();

        public string? ErrorMessage { get; set; }

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
                ErrorMessage = "User not found";
                return RedirectToPage("/Accounts/Login");
            }

            Input = user;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToPage("/Accounts/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Input.Id = userId;

                await _userService.UpdateAsync(Input);

                return RedirectToPage("/Landing");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}
