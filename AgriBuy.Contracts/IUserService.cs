using AgriBuy.Contracts.Dto;
using AgriBuy.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<UserDto?> GetByEmailAsync(string email);
        Task AddAsync(UserDto userDto, string password);
        Task UpdateAsync(UserDto userDto);
        Task<bool> UpdateProfileAsync(UserDto userDto);
        Task<bool> CheckPasswordAsync(string email, string password);
        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    }
}
