using System.Collections.Generic;
using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;

namespace Chat_Group_System.Services
{
    public interface IMessageService
    {
        Task<IEnumerable<Message>> GetRecentMessagesAsync(int conversationId, int skip=0, int take=50);
        Task<Message> SendTextMessageAsync(int conversationId, int senderId, string content);
        Task<Message> SendAttachmentMessageAsync(int conversationId, int senderId, string fileName, string fileUrl, long fileSize, MessageType type);
        Task<Message> SendSystemMessageAsync(int conversationId, string content);
        Task MarkAsReadAsync(int messageId, int userId);
    }
}