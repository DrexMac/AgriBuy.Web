using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Services.Notifications
{
    public interface IStoreNotificationService
    {
        Task NotifyOrderAddedAsync(Guid orderId);
        Task NotifyOrderPaidAsync(Guid orderId);
        Task NotifyOrderFailedAsync(Guid orderId);
    }
}