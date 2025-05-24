using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgriBuy.EntityFramework;

namespace AgriBuy.Services
{
    public class ProductService : IProductService
    {
        private readonly DefaultDbContext _context;
        private readonly IRepository<Product> _repository;
        private readonly IMapper _mapper;

        public ProductService(DefaultDbContext context, IRepository<Product> repository, IMapper mapper)
        {
            _context = context;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.Find(x => x.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<ProductDto?>(entity);
        }

        public async Task<IEnumerable<ProductDto>> GetByUserIdAsync(Guid userId)
        {
            var entities = await _repository
                .Find(x => x.Store != null && x.Store.UserId == userId)
                .Include(x => x.Store)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductDto>>(entities);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var entities = await _repository.All().ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(entities);
        }

        public async Task AddAsync(ProductDto productDto, Guid userId) 
        {
            var store = await _context.Stores
        .FirstOrDefaultAsync(s => s.UserId == userId);

            if (store == null)
            {
                
                store = new Store
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "My Store", 
                    Description = "Auto-created store for user"
                };
                await _context.Stores.AddAsync(store);
                await _context.SaveChangesAsync(); 
            }

            
            var entity = _mapper.Map<Product>(productDto);
            entity.Id = Guid.NewGuid();
            entity.StoreId = store.Id;

            await _context.Products.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductDto productDto)
        {
            var entity = _mapper.Map<Product>(productDto);
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
    }
}
