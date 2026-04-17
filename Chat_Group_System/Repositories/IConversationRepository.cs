using System.Collections.Generic;
using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;

namespace Chat_Group_System.Repositories
{
    public interface IConversationRepository
    {
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
        Task<Conversation?> GetByIdAsync(int id);
        Task<Conversation?> GetDirectMessageAsync(int userId1, int userId2);
        Task<Conversation> AddAsync(Conversation conversation, IEnumerable<int> participantIds);
        Task UpdateAsync(Conversation conversation);
    }
}