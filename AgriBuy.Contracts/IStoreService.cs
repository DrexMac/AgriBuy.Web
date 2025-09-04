using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;

namespace AgriBuy.Contracts
{
    public interface IStoreService : IService<Store>
    {
        
        Task<IEnumerable<StoreDto>> SearchAsync(string searchTerm);
        Task AddAsync(StoreDto store);
        Task UpdateAsync(StoreDto store);
        Task<Store?> GetStoreByUserIdAsync(Guid userId);
        Task AddAsync(StoreViewModel store);

        
    }
}
