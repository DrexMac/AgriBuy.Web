using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AgriBuy.Models.Enums;
using AgriBuy.Services.Notifications;

namespace AgriBuy.Services.Checkout
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IShoppingCartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IOrderItemService _orderItemService;
        private readonly IProductService _productService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<CheckoutService> _logger;
        private readonly IStoreNotificationService _notifications;

        private const string PayMongoCheckoutSessionsEndpoint = "https://api.paymongo.com/v1/checkout_sessions";

        public CheckoutService(
            IShoppingCartService cartService,
            IOrderService orderService,
            IOrderItemService orderItemService,
            IProductService productService,
            IHttpClientFactory httpFactory,
            IConfiguration config,
            ILogger<CheckoutService> logger,
            IStoreNotificationService notifications)
        {
            _cartService = cartService;
            _orderService = orderService;
            _orderItemService = orderItemService;
            _productService = productService;
            _httpFactory = httpFactory;
            _config = config;
            _logger = logger;
            _notifications = notifications;
        }

        public async Task<string> CreateCheckoutAndGetRedirectUrlAsync(
            Guid userId,
            Uri successUrl,
            Uri cancelUrl,
            CancellationToken ct = default)
        {
            var cartItems = (await _cartService.GetByUserIdAsync(userId)).ToList();
            if (!cartItems.Any())
                throw new InvalidOperationException("Your cart is empty.");

            var enriched = new List<(ShoppingCartDto Cart, Product Product)>();
            foreach (var ci in cartItems)
            {
                var prod = await _productService.GetByIdAsync(ci.ProductId);
                if (prod == null)
                    throw new InvalidOperationException($"Product not found: {ci.ProductId}");
                enriched.Add((ci, prod));
            }

            // Keep splitting Orders by store the same way as before — PayMongo doesn't split
            // payments by store, so all stores' items go into a single Checkout Session, and
            // every resulting Order is tagged with that one session's ID.
            var groups = enriched.GroupBy(x => x.Product.StoreId).ToList();
            decimal grandTotal = 0;
            var createdOrders = new List<OrderDto>();

            foreach (var group in groups)
            {
                var storeId = group.Key;
                var subtotal = group.Sum(x => x.Cart.Quantity * x.Cart.UnitPrice);
                grandTotal += subtotal;

                var orderDto = new OrderDto
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StoreId = storeId,
                    OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}",
                    OrderDate = DateTime.UtcNow,
                    IsPaid = false,
                    TotalPrice = subtotal
                };

                await _orderService.AddAsync(orderDto, userId);

                foreach (var (cart, product) in group)
                {
                    var oi = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderDto.Id,
                        ProductId = cart.ProductId,
                        Quantity = cart.Quantity,
                        UnitPrice = cart.UnitPrice,
                        UnitOfMeasure = cart.UnitOfMeasure,
                        ItemPrice = cart.Quantity * cart.UnitPrice
                    };
                    await _orderItemService.AddAsync(oi);
                }

                createdOrders.Add(orderDto);

                // Notification failures must not interrupt checkout creation.
                try { await _notifications.NotifyOrderAddedAsync(orderDto.Id); }
                catch (Exception ex) { _logger.LogWarning(ex, "Order-created notification failed for {OrderId}", orderDto.Id); }
            }

            var secretKey = _config["PayMongo:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("PayMongo secret key is missing.");

            // PayMongo amounts are integers in the smallest currency unit (centavos for PHP).
            static long ToCentavos(decimal value) => (long)Math.Round(value * 100m, MidpointRounding.AwayFromZero);

            var lineItems = enriched.Select(x => new
            {
                currency = "PHP",
                amount = ToCentavos(x.Cart.UnitPrice),
                name = x.Product.Name ?? x.Cart.ProductName,
                quantity = x.Cart.Quantity
            }).ToArray();

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        send_email_receipt = false,
                        show_description = true,
                        show_line_items = true,
                        description = "AgriBuy order",
                        line_items = lineItems,
                        // Adjust to whatever payment methods are enabled on your PayMongo account.
                        payment_method_types = new[] { "card", "gcash", "paymaya", "grab_pay" },
                        success_url = successUrl.ToString(),
                        cancel_url = cancelUrl.ToString(),
                        reference_number = $"ORDS-{DateTime.UtcNow:yyyyMMddHHmmss}"
                    }
                }
            };

            var client = _httpFactory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Post, PayMongoCheckoutSessionsEndpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            // PayMongo uses HTTP Basic auth with the secret key as the username and an empty password.
            var encodedKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretKey}:"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedKey);

            var resp = await client.SendAsync(req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("PayMongo checkout session creation failed: {Status} {Body}", resp.StatusCode, body);
                throw new Exception("Payment provider error.");
            }

            using var doc = JsonDocument.Parse(body);
            var data = doc.RootElement.GetProperty("data");
            var checkoutSessionId = data.GetProperty("id").GetString();
            var attributes = data.GetProperty("attributes");
            var checkoutUrl = attributes.GetProperty("checkout_url").GetString()
                ?? throw new Exception("PayMongo response did not include a checkout_url.");

            // Tag every order created for this cart with the Checkout Session ID so the webhook
            // can match the event back to these orders.
            foreach (var o in createdOrders)
            {
                try
                {
                    o.ORNumber = checkoutSessionId;
                    await _orderService.DetachedUpdateAsync(o);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Safe update failed for order {OrderNumber}", o.OrderNumber);
                }
            }

            return checkoutUrl;
        }

        public async Task HandlePaymentNotificationAsync(PaymentNotificationDto notification, CancellationToken ct = default)
        {
            if (notification == null) return;

            // We only act on the checkout-session-level success event. PayMongo checkout sessions
            // don't push a distinct "failed" webhook the way PayMaya did — a customer who cancels
            // or fails to pay is simply redirected back to cancel_url, and no stock was ever
            // reserved for the unpaid order, so there's nothing to roll back here.
            var isCheckoutSuccess = string.Equals(
                notification.EventType,
                "checkout_session.payment.paid",
                StringComparison.OrdinalIgnoreCase);

            if (!isCheckoutSuccess || string.IsNullOrWhiteSpace(notification.CheckoutSessionId))
            {
                _logger.LogInformation("Ignoring PayMongo event {EventType}", notification.EventType);
                return;
            }

            var orders = await _orderService.GetAllAsync();
            var matched = orders.Where(o => o.ORNumber == notification.CheckoutSessionId).ToList();

            if (!matched.Any())
            {
                _logger.LogWarning("No orders matched PayMongo checkout session {CheckoutSessionId}", notification.CheckoutSessionId);
                return;
            }

            foreach (var order in matched)
            {
                if (order.IsPaid) continue; // idempotency: PayMongo may retry webhook delivery

                order.IsPaid = true;
                order.PayDate = DateTime.UtcNow;
                order.Status = OrderStatus.Processing; // paid, seller now needs to process/ship it
                await _orderService.DetachedUpdateAsync(order);

                try { await _notifications.NotifyOrderPaidAsync(order.Id); }
                catch (Exception ex) { _logger.LogWarning(ex, "Payment notification failed for {OrderId}", order.Id); }

                var items = await _orderItemService.GetByOrderIdAsync(order.Id);
                foreach (var item in items)
                {
                    var product = await _productService.GetByIdAsync(item.ProductId);
                    if (product != null && product.Quantity >= item.Quantity)
                    {
                        product.Quantity -= item.Quantity;
                        await _productService.UpdateAsync(product);
                    }
                }
            }

            var userId = matched.First().UserId;
            await _cartService.ClearCartAsync(userId);
        }
    }
}