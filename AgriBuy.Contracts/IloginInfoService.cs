using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface ILoginInfoService
    {
        Task<LoginInfoDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<LoginInfoDto>> GetByUserIdAsync(Guid userId);
        Task AddAsync(LoginInfoDto model);
        Task UpdateAsync(LoginInfoDto model);
        Task DeleteAsync(Guid id);
    }
}
