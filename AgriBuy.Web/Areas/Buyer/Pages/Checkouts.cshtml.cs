using AgriBuy.Services.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Buyer.Pages
{
    public class CheckoutsModel : PageModel
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IConfiguration _config;
        private readonly ILogger<CheckoutsModel> _logger;

        public CheckoutsModel(ICheckoutService checkoutService, IConfiguration config, ILogger<CheckoutsModel> logger)
        {
            _checkoutService = checkoutService;
            _config = config;
            _logger = logger;
        }

        public void OnGet()
        {
            // Optional: render a simple "Checkout Processing" view
        }

        // PayMongo webhook handler
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostNotifyAsync()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var rawJson = await reader.ReadToEndAsync();

                _logger.LogInformation("Received PayMongo webhook on /Buyer/Checkouts: {Json}", rawJson);

                if (!Request.Headers.TryGetValue("Paymongo-Signature", out var signatureHeader))
                {
                    _logger.LogWarning("PayMongo webhook rejected: missing Paymongo-Signature header.");
                    return BadRequest(new { error = "Missing signature" });
                }

                var webhookSecret = _config["PayMongo:WebhookSecretKey"];
                if (string.IsNullOrWhiteSpace(webhookSecret))
                {
                    _logger.LogError("PayMongo:WebhookSecretKey is not configured.");
                    return StatusCode(500, new { error = "Webhook secret not configured" });
                }

                if (!TryVerifySignature(rawJson, signatureHeader.ToString(), webhookSecret))
                {
                    _logger.LogWarning("PayMongo webhook rejected: signature verification failed.");
                    return StatusCode(401, new { error = "Invalid signature" });
                }

                var notification = ParseNotification(rawJson);
                if (notification == null)
                {
                    _logger.LogWarning("Invalid webhook payload.");
                    return BadRequest(new { error = "Invalid payload" });
                }

                await _checkoutService.HandlePaymentNotificationAsync(notification);
                _logger.LogInformation("Webhook processed successfully.");
                return new JsonResult(new { message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayMongo webhook");
                return StatusCode(500, new { error = ex.Message });
            }
        }

    
        private static bool TryVerifySignature(string rawBody, string signatureHeader, string webhookSecret)
        {
            string? timestamp = null;
            string? testSig = null;
            string? liveSig = null;

            foreach (var part in signatureHeader.Split(','))
            {
                var kv = part.Split('=', 2);
                if (kv.Length != 2) continue;

                switch (kv[0].Trim())
                {
                    case "t": timestamp = kv[1].Trim(); break;
                    case "te": testSig = kv[1].Trim(); break;
                    case "li": liveSig = kv[1].Trim(); break;
                }
            }

            if (string.IsNullOrEmpty(timestamp))
                return false;

            var signedPayload = $"{timestamp}.{rawBody}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
            var computedSig = Convert.ToHexString(computedHash).ToLowerInvariant();

            var matchesTest = !string.IsNullOrEmpty(testSig) &&
                CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computedSig), Encoding.UTF8.GetBytes(testSig));
            var matchesLive = !string.IsNullOrEmpty(liveSig) &&
                CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(computedSig), Encoding.UTF8.GetBytes(liveSig));

            return matchesTest || matchesLive;
        }

        private static PaymentNotificationDto? ParseNotification(string rawJson)
        {
            using var doc = JsonDocument.Parse(rawJson);
            var eventAttributes = doc.RootElement.GetProperty("data").GetProperty("attributes");

            var eventType = eventAttributes.GetProperty("type").GetString();
            var inner = eventAttributes.GetProperty("data");
            var innerType = inner.GetProperty("type").GetString();
            var innerId = inner.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;

            string? referenceNumber = null;
            if (inner.TryGetProperty("attributes", out var innerAttrs) &&
                innerAttrs.TryGetProperty("reference_number", out var refProp))
            {
                referenceNumber = refProp.GetString();
            }

            // For checkout_session.payment.paid, inner.id IS the checkout session ID (cs_xxx),
            // which is what CheckoutService stored on the Order.
            var checkoutSessionId = innerType == "checkout_session" ? innerId : null;

            return new PaymentNotificationDto
            {
                EventType = eventType,
                CheckoutSessionId = checkoutSessionId,
                ReferenceNumber = referenceNumber,
                RawPayload = rawJson
            };
        }
    }
}