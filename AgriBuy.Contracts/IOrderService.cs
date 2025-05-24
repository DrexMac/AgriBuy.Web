using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface IOrderService
    {
        Task<OrderDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<OrderDto>> GetByUserIdAsync(Guid userId);

        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task AddAsync(OrderDto orderDto);
        Task UpdateAsync(OrderDto orderDto);
        Task DeleteAsync(Guid id);
    }
}
