using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgriBuy.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<Order> _repository;
        private readonly IMapper _mapper;

        public OrderService(IRepository<Order> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var entities = await _repository.All().Include(x => x.OrderItems).ToListAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(entities);
        }

        public async Task<OrderDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.Find(x => x.Id == id).Include(x => x.OrderItems).FirstOrDefaultAsync();
            return _mapper.Map<OrderDto?>(entity);
        }

        public async Task AddAsync(OrderDto orderDto)
        {
            var entity = _mapper.Map<Order>(orderDto);
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderDto orderDto)
        {
            var entity = _mapper.Map<Order>(orderDto);
            _repository.Update(entity);
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

        public async Task<IEnumerable<OrderDto>> GetByUserIdAsync(Guid userId)
        {
            var entities = await _repository.Find(x => x.UserId == userId).Include(x => x.OrderItems).ToListAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(entities);
        }

        public async Task AddAsync(OrderDto orderDto, Guid userId)
        {
            var entity = _mapper.Map<Order>(orderDto);
            entity.UserId = userId;
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }
    }
}