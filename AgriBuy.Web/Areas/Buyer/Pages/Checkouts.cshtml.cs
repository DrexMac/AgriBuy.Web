using AgriBuy.Services.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Buyer.Pages
{
    public class CheckoutsModel : PageModel
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ILogger<CheckoutsModel> _logger;

        public CheckoutsModel(ICheckoutService checkoutService, ILogger<CheckoutsModel> logger)
        {
            _checkoutService = checkoutService;
            _logger = logger;
        }

        public void OnGet()
        {
            // Optional: render a simple "Checkout Processing" view
        }

        //  PayMaya webhook handler
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostNotifyAsync()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();

                _logger.LogInformation(" Received PayMaya webhook on /Buyer/Checkouts: {Json}", json);

                var notification = JsonSerializer.Deserialize<PaymentNotificationDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (notification == null)
                {
                    _logger.LogWarning(" Invalid webhook payload.");
                    return BadRequest(new { error = "Invalid payload" });
                }

                await _checkoutService.HandlePaymentNotificationAsync(notification);
                _logger.LogInformation(" Webhook processed successfully.");
                return new JsonResult(new { message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayMaya webhook");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
