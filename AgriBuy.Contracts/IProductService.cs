using AgriBuy.Contracts.Dto;
using AgriBuy.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface IProductService
    {
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductDto>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task AddAsync(ProductDto productDto, Guid userId);
        Task UpdateAsync(ProductDto productDto);
        Task DeleteAsync(Guid id); 
    }
}
