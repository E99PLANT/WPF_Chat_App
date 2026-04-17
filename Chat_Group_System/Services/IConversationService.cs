using System.Collections.Generic;
using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;

namespace Chat_Group_System.Services
{
    public interface IConversationService
    {
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
        Task<Conversation?> GetConversationByIdAsync(int conversationId);
        Task<Conversation> CreateGroupChatAsync(int creatorId, string groupName, IEnumerable<int> memberIds);
        Task<Conversation> CreateOrGetDirectMessageAsync(int user1Id, int user2Id);
        Task UpdateLastMessagePreviewAsync(int conversationId, string previewText);
    }
}