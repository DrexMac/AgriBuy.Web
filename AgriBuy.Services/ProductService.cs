using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        // --- IService<Product> implementation (Seller CRUD)
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _repository.All().Include(p => p.Store).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _repository.Find(x => x.Id == id).Include(p => p.Store).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Product entity)
        {
            entity.Id = Guid.NewGuid();
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product entity)
        {
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

        // --- IProductService specific methods ---
        public async Task<IEnumerable<ProductDto>> GetByUserIdAsync(Guid userId)
        {
            var entities = await _repository
                .Find(x => x.Store != null && x.Store.UserId == userId)
                .Include(x => x.Store)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductDto>>(entities);
        }

        public async Task AddAsync(ProductDto productDto, Guid userId)
        {
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.UserId == userId);
            if (store == null)
            {
                store = new Store
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "My Store"
                };
                await _context.Stores.AddAsync(store);
                await _context.SaveChangesAsync();
            }

            var entity = _mapper.Map<Product>(productDto);
            entity.Id = Guid.NewGuid();
            entity.StoreId = store.Id;
            entity.UserId = userId;

            await _context.Products.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductDto productDto)
        {
            var entity = _mapper.Map<Product>(productDto);
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
        }

        // --- Buyer pages ---
        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _repository.All().Include(p => p.Store).ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
    }
}
