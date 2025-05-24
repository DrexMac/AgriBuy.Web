using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using AgriBuy.Models.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AgriBuy.Services
{
    public class UserService : IUserService
    {
        private readonly DefaultDbContext _context;
        private readonly IMapper _mapper;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(DefaultDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task AddAsync(UserDto userDto, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password must be provided", nameof(password));

            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = HashPassword(user, password);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<bool> CheckPasswordAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return false;

            return VerifyPassword(user, password);
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            if (!VerifyPassword(user, currentPassword))
                throw new UnauthorizedAccessException("Current password is incorrect");

            user.PasswordHash = HashPassword(user, newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserDto userDto)
        {
            var user = await _context.Users.FindAsync(userDto.Id);
            if (user == null) throw new ArgumentException("User not found");

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.EmailAddress = userDto.EmailAddress;

            if (Enum.TryParse<UserRole>(userDto.Role, true, out var role))
            {
                user.Role = role;
            }
            else
            {
                throw new ArgumentException("Invalid role specified");
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateProfileAsync(UserDto userDto)
        {
            var user = await _context.Users.FindAsync(userDto.Id);
            if (user == null)
                return false;

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.EmailAddress = userDto.EmailAddress;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }



        private string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        private bool VerifyPassword(User user, string password)
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
                return false;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}
