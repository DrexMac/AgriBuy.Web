using AgriBuy.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Services.Notifications
{
    public class StoreNotificationService : IStoreNotificationService
    {
        private readonly IOrderService _orderService;
        private readonly IStoreService _storeService;
        private readonly ILogger<StoreNotificationService> _logger;

        public StoreNotificationService(
            IOrderService orderService,
            IStoreService storeService,
            ILogger<StoreNotificationService> logger)
        {
            _orderService = orderService;
            _storeService = storeService;
            _logger = logger;
        }

        public async Task NotifyOrderAddedAsync(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(orderId);
                if (order == null) return;

                var store = await _storeService.GetByIdAsync(order.StoreId);
                if (store == null) return;

                _logger.LogInformation("Notify Store {StoreId} ({StoreName}) : Order {OrderNumber} CREATED.",
                    store.Id, store.Name, order.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to NotifyOrderAddedAsync for {OrderId}", orderId);
            }
        }

        public async Task NotifyOrderPaidAsync(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(orderId);
                if (order == null) return;

                var store = await _storeService.GetByIdAsync(order.StoreId);
                if (store == null) return;

                _logger.LogInformation("Notify Store {StoreId} ({StoreName}) : Order {OrderNumber} PAID.",
                    store.Id, store.Name, order.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to NotifyOrderPaidAsync for {OrderId}", orderId);
            }
        }

        public async Task NotifyOrderFailedAsync(Guid orderId)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(orderId);
                if (order == null) return;

                var store = await _storeService.GetByIdAsync(order.StoreId);
                if (store == null) return;

                _logger.LogInformation("Notify Store {StoreId} ({StoreName}) : Order {OrderNumber} FAILED/CANCELLED.",
                    store.Id, store.Name, order.OrderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to NotifyOrderFailedAsync for {OrderId}", orderId);
            }
        }
    }
}