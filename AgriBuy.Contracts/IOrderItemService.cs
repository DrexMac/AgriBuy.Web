using AgriBuy.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface IOrderItemService
    {
        Task<OrderItem?> GetByIdAsync(Guid id);
        Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task AddAsync(OrderItem orderItem);
        Task UpdateAsync(OrderItem orderItem);
        Task DeleteAsync(Guid id);
    }
}
