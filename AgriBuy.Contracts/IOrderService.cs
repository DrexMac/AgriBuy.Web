using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface IOrderService : IService<OrderDto>
    {
        Task<IEnumerable<OrderDto>> GetByUserIdAsync(Guid userId);
        Task AddAsync(OrderDto orderDto, Guid userId);
    }
}
