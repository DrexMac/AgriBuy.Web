using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface ICategoryService : IService<Category>
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task AddAsync(CategoryDto categoryDto);
        Task UpdateAsync(CategoryDto categoryDto);

        Task<IEnumerable<Category>> GetRootCategoriesAsync();
    }
}
