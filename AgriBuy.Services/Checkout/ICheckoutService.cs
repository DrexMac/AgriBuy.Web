using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgriBuy.Services.Checkout
{
    public interface ICheckoutService
    {
        /// <summary>
        /// Creates an Order from the user's ShoppingCart and returns a PayMongo checkout URL.
        /// </summary>
        Task<string> CreateCheckoutAndGetRedirectUrlAsync(
            Guid userId,
            Uri successUrl,
            Uri cancelUrl,
            CancellationToken ct = default);

        /// <summary>
        /// Handles a verified PayMongo webhook event and updates Order status.
        /// </summary>
        Task HandlePaymentNotificationAsync(PaymentNotificationDto notification, CancellationToken ct = default);
    }

    public class PaymentNotificationDto
    {
        /// <summary>
        /// PayMongo Checkout Session ID (e.g. cs_xxx). This is what we stored on the Order
        /// when the checkout session was created, so it's how we match the event back to Orders.
        /// </summary>
        public string? CheckoutSessionId { get; set; }

        /// <summary>
        /// The event's reference_number, if PayMongo returned one on the checkout session/payment.
        /// </summary>
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// The PayMongo event type, e.g. "checkout_session.payment.paid", "payment.paid", "payment.failed".
        /// </summary>
        public string? EventType { get; set; }

        public string? RawPayload { get; set; }
    }
}