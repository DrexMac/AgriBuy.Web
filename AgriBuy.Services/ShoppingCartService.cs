using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IRepository<ShoppingCart> _repository;
        private readonly IMapper _mapper;

        public ShoppingCartService(IRepository<ShoppingCart> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ShoppingCartDto>> GetByUserIdAsync(Guid userId)
        {
            var entities = await _repository.Find(x => x.UserId == userId)
                                            .Include(x => x.Product)
                                            .Where(x => x.Product != null && !x.Product.IsDeleted)
                                            .AsNoTracking()
                                            .ToListAsync();

            return _mapper.Map<IEnumerable<ShoppingCartDto>>(entities);
        }

        public async Task AddAsync(ShoppingCartDto model)
        {
            var entity = _mapper.Map<ShoppingCart>(model);
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(ShoppingCartDto model)
        {
            var entity = await _repository.Find(x => x.Id == model.Id).FirstOrDefaultAsync();
            if (entity != null)
            {
                entity.Quantity = model.Quantity;
                await _repository.SaveChangesAsync();
            }
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

        public async Task ClearCartAsync(Guid userId)
        {
            var items = await _repository.Find(x => x.UserId == userId).ToListAsync();
            if (items.Any())
            {
                _repository.Delete(items.ToArray());
                await _repository.SaveChangesAsync();
            }
        }
    }
}
