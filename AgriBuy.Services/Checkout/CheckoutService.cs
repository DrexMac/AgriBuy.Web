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

        public CheckoutService(
            IShoppingCartService cartService,
            IOrderService orderService,
            IOrderItemService orderItemService,
            IProductService productService,
            IHttpClientFactory httpFactory,
            IConfiguration config,
            ILogger<CheckoutService> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _orderItemService = orderItemService;
            _productService = productService;
            _httpFactory = httpFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<string> CreateCheckoutAndGetRedirectUrlAsync(
            Guid userId,
            Uri successUrl,
            Uri failureUrl,
            CancellationToken ct = default)
        {
            var cartItems = (await _cartService.GetByUserIdAsync(userId)).ToList();
            if (!cartItems.Any())
                throw new InvalidOperationException("Your cart is empty.");

            // group cart items by store
            var enriched = new List<(ShoppingCartDto Cart, Product Product)>();
            foreach (var ci in cartItems)
            {
                var prod = await _productService.GetByIdAsync(ci.ProductId);
                if (prod == null)
                    throw new InvalidOperationException($"Product not found: {ci.ProductId}");
                enriched.Add((ci, prod));
            }

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

                    //  Do not deduct stock yet (only after payment success)
                }

                createdOrders.Add(orderDto);
            }

            // ---- Payment Payload ----
            var payload = new
            {
                totalAmount = new { value = grandTotal.ToString("0.00"), currency = "PHP" },
                requestReferenceNumber = $"ORDS-{DateTime.UtcNow:yyyyMMddHHmmss}",
                buyer = new { firstName = "Buyer" },
                items = enriched.Select(x => new
                {
                    name = x.Product.Name ?? x.Cart.ProductName,
                    quantity = x.Cart.Quantity,
                    amount = new { value = x.Cart.UnitPrice.ToString("0.00") },
                    totalAmount = new { value = (x.Cart.UnitPrice * x.Cart.Quantity).ToString("0.00") }
                }),
                redirectUrl = new
                {
                    success = successUrl.ToString(),
                    failure = failureUrl.ToString(),
                    cancel = failureUrl.ToString()
                }
            };

            // ---- Payment Simulation or PayMaya ----
            var mayaEndpoint = _config["Maya:Endpoint"];
            var mayaPublicKey = _config["Maya:PublicKey"];
            string checkoutUrl = successUrl.ToString();
            string? checkoutId = null;

            if (!string.IsNullOrWhiteSpace(mayaEndpoint) && !string.IsNullOrWhiteSpace(mayaPublicKey))
            {
                var client = _httpFactory.CreateClient("MayaClient");
                var req = new HttpRequestMessage(HttpMethod.Post, mayaEndpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{mayaPublicKey}:")));

                var resp = await client.SendAsync(req, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("PayMaya checkout failed: {Status} {Body}", resp.StatusCode, body);
                    throw new Exception("Payment provider error.");
                }

                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("redirectUrl", out var redirectUrlProp))
                    checkoutUrl = redirectUrlProp.GetString() ?? checkoutUrl;
                if (doc.RootElement.TryGetProperty("id", out var idProp))
                    checkoutId = idProp.GetString();
            }
            else
            {
                checkoutId = $"LOCAL-{Guid.NewGuid():N}";
            }

            // ---- Update Orders with ORNumber safely ----
            foreach (var o in createdOrders)
            {
                try
                {
                    o.ORNumber = checkoutId;
                    await _orderService.DetachedUpdateAsync(o);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Safe update failed for order {OrderNumber}", o.OrderNumber);
                }
            }

            //  Removed cart clearing here – handled after successful payment
            return checkoutUrl;
        }

        public async Task HandlePaymentNotificationAsync(PaymentNotificationDto notification, CancellationToken ct = default)
        {
            if (notification == null) return;

            var orders = await _orderService.GetAllAsync();
            var matched = orders.Where(o =>
                o.ORNumber == notification.CheckoutId ||
                o.OrderNumber == notification.RequestReferenceNumber).ToList();

            if (!matched.Any()) return;

            var success = (notification.Status ?? "").ToUpperInvariant().Contains("SUCCESS");

            if (success)
            {
                //  Mark orders as paid, deduct stock, clear cart
                foreach (var order in matched)
                {
                    order.IsPaid = true;
                    order.PayDate = DateTime.UtcNow;
                    await _orderService.DetachedUpdateAsync(order);

                    // Deduct stock only after successful payment
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

                // Clear buyer cart after successful payment
                var userId = matched.First().UserId;
                await _cartService.ClearCartAsync(userId);
            }
            else
            {
                //  Payment failed → optionally restore stock
                foreach (var order in matched)
                {
                    var items = await _orderItemService.GetByOrderIdAsync(order.Id);
                    foreach (var item in items)
                    {
                        var product = await _productService.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Quantity += item.Quantity;
                            await _productService.UpdateAsync(product);
                        }
                    }

                    order.IsPaid = false;
                    await _orderService.DetachedUpdateAsync(order);
                }
            }
        }
    }
}

