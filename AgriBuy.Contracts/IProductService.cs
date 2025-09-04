using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using AgriBuy.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface IProductService : IService<Product>
    {
        Task<IEnumerable<ProductDto>> GetByUserIdAsync(Guid userId);
        Task AddAsync(ProductDto productDto, Guid userId);
    }
}
