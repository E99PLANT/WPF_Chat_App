using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;

namespace Chat_Group_System.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<bool> RegisterAsync(string fullName, string email, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByEmailOrNameAsync(string searchTerm);
    }
}