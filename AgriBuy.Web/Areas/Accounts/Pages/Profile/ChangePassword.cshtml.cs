using System;
using System.Threading.Tasks;
using AgriBuy.Contracts;
using AgriBuy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgriBuy.Web.Areas.Accounts.Pages.Profile
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IUserService _userService;

        public ChangePasswordModel(IUserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public ChangePasswordVm Input { get; set; } = new ChangePasswordVm();

        public string? ErrorMessage { get; set; }

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
                await _userService.ChangePasswordAsync(userId, Input.CurrentPassword, Input.NewPassword);
                return RedirectToPage("/Landing"); 
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}
