using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface IShoppingCartService
    {
        Task<IEnumerable<ShoppingCartDto>> GetByUserIdAsync(Guid userId);
        Task AddAsync(ShoppingCartDto item);
        Task UpdateAsync(ShoppingCartDto item);
        Task DeleteAsync(Guid id);
        Task ClearCartAsync(Guid userId);

    }
}

