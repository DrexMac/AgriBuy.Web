using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgriBuy.Services.Checkout
{
    public interface ICheckoutService
    {
        /// <summary>
        /// Creates an Order from the user's ShoppingCart and returns a PayMaya checkout URL.
        /// </summary>
        Task<string> CreateCheckoutAndGetRedirectUrlAsync(
            Guid userId,
            Uri successUrl,
            Uri failureUrl,
            CancellationToken ct = default);

        /// <summary>
        /// Handles PayMaya webhook callback (success/failure) and updates Order status.
        /// </summary>
        Task HandlePaymentNotificationAsync(PaymentNotificationDto notification, CancellationToken ct = default);
    }

    public class PaymentNotificationDto
    {
        public string? CheckoutId { get; set; }
        public string? RequestReferenceNumber { get; set; }
        public string? Status { get; set; }
        public string? RawPayload { get; set; }
    }
}