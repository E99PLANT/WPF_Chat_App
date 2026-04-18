using System;
using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;
using Chat_Group_System.Repositories;

namespace Chat_Group_System.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive) return null;

            if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task<bool> RegisterAsync(string fullName, string email, string password)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null) return false;

            var user = new User
            {
                DisplayName = fullName,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = "Member",
                IsActive = true,
                CreatedAt = Chat_Group_System.Helpers.TimeHelper.NowVN,
                UpdatedAt = Chat_Group_System.Helpers.TimeHelper.NowVN
            };

            await _userRepository.AddAsync(user);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash)) return false;

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = Chat_Group_System.Helpers.TimeHelper.NowVN;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        // Password hashing method using BCrypt
        private string HashPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword);
        }
    }
}
