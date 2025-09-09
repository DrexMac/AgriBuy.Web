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
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); 
            }

            try
            {
                
                await _userService.AddAsync(new Contracts.Dto.UserDto
                {
                    EmailAddress = Input.EmailAddress,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    
                }, Input.Password);

                
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
