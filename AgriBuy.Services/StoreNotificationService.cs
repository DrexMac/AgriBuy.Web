using AgriBuy.Contracts;
using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgriBuy.Services.Notifications
{
    public class StoreNotificationService : IStoreNotificationService
    {
        private readonly IOrderService _orderService;
        private readonly IStoreService _storeService;
        private readonly DefaultDbContext _context;
        private readonly ILogger<StoreNotificationService> _logger;

        public StoreNotificationService(
            IOrderService orderService,
            IStoreService storeService,
            DefaultDbContext context,
            ILogger<StoreNotificationService> logger)
        {
            _orderService = orderService;
            _storeService = storeService;
            _context = context;
            _logger = logger;
        }

        public async Task NotifyOrderAddedAsync(Guid orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null) return;

            var store = await _storeService.GetByIdAsync(order.StoreId);
            if (store == null) return;

            await AddAsync(store.UserId, order.Id, $"New order {order.OrderNumber} was placed.");
            await AddAsync(order.UserId, order.Id, $"Your order {order.OrderNumber} was placed successfully.");
        }

        public async Task NotifyOrderPaidAsync(Guid orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null) return;

            var store = await _storeService.GetByIdAsync(order.StoreId);
            if (store == null) return;

            await AddAsync(store.UserId, order.Id, $"Payment received for order {order.OrderNumber}.");
            await AddAsync(order.UserId, order.Id, $"Payment for order {order.OrderNumber} was confirmed.");
        }

        public async Task NotifyOrderFailedAsync(Guid orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null) return;

            var store = await _storeService.GetByIdAsync(order.StoreId);
            if (store == null) return;

            await AddAsync(store.UserId, order.Id, $"Order {order.OrderNumber} payment failed or was cancelled.");
            await AddAsync(order.UserId, order.Id, $"Payment for order {order.OrderNumber} failed or was cancelled.");
        }

        public async Task NotifyOrderDeliveredAsync(Guid orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId);
            if (order == null) return;
            await AddAsync(order.UserId, order.Id, $"Order {order.OrderNumber} was marked delivered. View your order for the seller's delivery proof.");
        }

        private async Task AddAsync(Guid recipientUserId, Guid orderId, string message)
        {
            if (recipientUserId == Guid.Empty) return;

            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = recipientUserId,
                OrderId = orderId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("Stored notification for user {UserId} about order {OrderId}", recipientUserId, orderId);
        }
    }
}