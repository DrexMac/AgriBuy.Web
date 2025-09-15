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
    public class CategoryService : ICategoryService
    {
        private readonly DefaultDbContext _context;
        private readonly IRepository<Category> _repository;
        private readonly IMapper _mapper;

        public CategoryService(DefaultDbContext context, IRepository<Category> repository, IMapper mapper)
        {
            _context = context;
            _repository = repository;
            _mapper = mapper;
        }

        // --- IService<Category> implementation ---
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _repository.All().Include(c => c.Children).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _repository.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddAsync(Category entity)
        {
            entity.Id = Guid.NewGuid();
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category entity)
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

        // --- ICategoryService specific methods ---
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _repository.All().ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

      

        public async Task AddAsync(CategoryDto categoryDto)
        {
            var entity = _mapper.Map<Category>(categoryDto);
            entity.Id = Guid.NewGuid();
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(CategoryDto categoryDto)
        {
            var entity = _mapper.Map<Category>(categoryDto);
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _repository.Find(c => c.ParentId == null).ToListAsync();
        }
    }
}
