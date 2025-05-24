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
    public class LoginInfoService : ILoginInfoService
    {
        private readonly IRepository<LoginInfo> _repository;
        private readonly IMapper _mapper;

        public LoginInfoService(IRepository<LoginInfo> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<LoginInfoDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.Find(x => x.Id == id).FirstOrDefaultAsync();
            return _mapper.Map<LoginInfoDto?>(entity);
        }

        public async Task<IEnumerable<LoginInfoDto>> GetByUserIdAsync(Guid userId)
        {
            var entities = await _repository.Find(x => x.UserId == userId).ToListAsync();
            return _mapper.Map<IEnumerable<LoginInfoDto>>(entities);
        }

        public async Task AddAsync(LoginInfoDto model)
        {
            var entity = _mapper.Map<LoginInfo>(model);
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateAsync(LoginInfoDto model)
        {
            var entity = _mapper.Map<LoginInfo>(model);
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
