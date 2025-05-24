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
    public class StoreService : IStoreService
    {
        private readonly IRepository<Store> _repository;
        private readonly IMapper _mapper;

        public StoreService(IRepository<Store> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<StoreDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.Find(x => x.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<StoreDto?>(entity);
        }

        public async Task<IEnumerable<StoreDto>> GetAllAsync()
        {
            var entities = await _repository.All().ToListAsync();
            return _mapper.Map<IEnumerable<StoreDto>>(entities);
        }

        public async Task<IEnumerable<StoreDto>> SearchAsync(string searchTerm)
        {
            var entities = await _repository.Find(x =>
                (x.Name != null && x.Name.Contains(searchTerm)) ||
                (x.Description != null && x.Description.Contains(searchTerm))
            ).ToListAsync();

            return _mapper.Map<IEnumerable<StoreDto>>(entities);
        }


        public async Task AddAsync(StoreDto store)
        {
            var entity = _mapper.Map<Store>(store);
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(StoreDto store)
        {
            var entity = _mapper.Map<Store>(store);
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
