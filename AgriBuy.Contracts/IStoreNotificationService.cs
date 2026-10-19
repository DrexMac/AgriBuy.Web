using System;
using System.Threading.Tasks;

namespace AgriBuy.Services.Notifications
{
    public interface IStoreNotificationService
    {
        Task NotifyOrderAddedAsync(Guid orderId);
        Task NotifyOrderPaidAsync(Guid orderId);
        Task NotifyOrderFailedAsync(Guid orderId);
        Task NotifyOrderDeliveredAsync(Guid orderId);
    }
}