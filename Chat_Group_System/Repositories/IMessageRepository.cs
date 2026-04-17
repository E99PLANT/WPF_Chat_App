using System.Collections.Generic;
using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;

namespace Chat_Group_System.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetMessagesByConversationIdAsync(int conversationId, int skip = 0, int take = 50);
        Task<Message> AddMessageAsync(Message message);
        Task MarkMessageAsReadAsync(int messageId, int userId);
        Task<Message?> GetMessageByIdAsync(int id);
    }
}