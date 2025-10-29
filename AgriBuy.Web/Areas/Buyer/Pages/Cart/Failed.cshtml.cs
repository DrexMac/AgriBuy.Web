using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AgriBuy.Web.Areas.Buyer.Pages.Cart
{
    public class FailedModel : PageModel
    {
        private readonly ILogger<FailedModel> _logger;

        public FailedModel(ILogger<FailedModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogWarning(" Checkout Failed Page loaded.");
        }
    }
}
