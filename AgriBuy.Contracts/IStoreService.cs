using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;

namespace AgriBuy.Contracts
{
    public interface IStoreService
    {
        Task<StoreDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<StoreDto>> GetAllAsync();
        Task<IEnumerable<StoreDto>> SearchAsync(string searchTerm);
        Task AddAsync(StoreDto store);
        Task UpdateAsync(StoreDto store);
        Task DeleteAsync(Guid id);
    }
}
