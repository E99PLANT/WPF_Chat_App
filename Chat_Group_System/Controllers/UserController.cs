using System;
using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;
using Chat_Group_System.Services;

namespace Chat_Group_System.Controllers
{
    public class UserController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<(bool Success, string Message, User? SessionUser)> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Please enter email and password.", null);

            try
            {
                var user = await _userService.AuthenticateAsync(email, password);
                if (user != null)
                {
                    return (true, "Login successful.", user);
                }
                return (false, "Invalid email or password.", null);
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string fullName, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "All fields are required.");

            if (password != confirmPassword)
                return (false, "Passwords do not match.");

            try
            {
                bool result = await _userService.RegisterAsync(fullName, email, password);
                if (result)
                {
                    return (true, "Account created successfully.");
                }
                return (false, "Email is already taken.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred during registration: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string confirmNewPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                return (false, "All fields are required.");

            if (newPassword != confirmNewPassword)
                return (false, "New passwords do not match.");

            try
            {
                bool result = await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);
                if (result)
                {
                    return (true, "Password changed successfully.");
                }
                return (false, "Incorrect current password, or user not found.");
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }
    }
}