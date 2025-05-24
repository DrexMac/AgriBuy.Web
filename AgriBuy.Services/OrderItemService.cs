using AgriBuy.Contracts;
using AgriBuy.Models.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgriBuy.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IRepository<OrderItem> _repository;
        private readonly IMapper _mapper;

        public OrderItemService(IRepository<OrderItem> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<OrderItem?> GetByIdAsync(Guid id)
        {
            return await _repository.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            return await _repository.Find(x => x.OrderId == orderId).ToListAsync();
        }

        public async Task AddAsync(OrderItem orderItem)
        {
            await _repository.AddAsync(orderItem);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderItem orderItem)
        {
            _repository.Update(orderItem);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _repository.Find(x => x.Id == id).FirstOrDefaultAsync();

            if (entity != null)
            {
                _repository.Delete(entity);
                await _repository.SaveChangesAsync();
            }
        }
    }
}
