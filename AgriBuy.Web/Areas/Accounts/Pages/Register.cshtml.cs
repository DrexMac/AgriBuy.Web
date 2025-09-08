using AgriBuy.Contracts;
using AgriBuy.Models.ViewModels;
using AgriBuy.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Accounts.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public RegistrationVm Input { get; set; } = new RegistrationVm();

        public string? ErrorMessage { get; set; }
        private readonly IUserService _userService;

        public RegisterModel(IUserService userService)
        {
            _userService = userService;
        }

        public void OnGet()
        {
            // This method is called on GET requests
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // Return the page with validation errors
            }

            try
            {
                // Create a new user using the UserService
                await _userService.AddAsync(new Contracts.Dto.UserDto
                {
                    EmailAddress = Input.EmailAddress,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    // You may want to hash the password before saving
                }, Input.Password);

                // Redirect to a success page or login page
                return RedirectToPage("/Login", new { area = "Accounts" });
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}
