using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;
using Chat_Group_System.Repositories;

namespace Chat_Group_System.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationService _conversationService;

        public MessageService(IMessageRepository messageRepository, IConversationService conversationService)
        {
            _messageRepository = messageRepository;
            _conversationService = conversationService;
        }

        public async Task<IEnumerable<Message>> GetRecentMessagesAsync(int conversationId, int skip=0, int take=50)
        {
            return await _messageRepository.GetMessagesByConversationIdAsync(conversationId, skip, take);
        }

        public async Task<Message> SendTextMessageAsync(int conversationId, int senderId, string content)
        {
            var msg = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };

            var savedMessage = await _messageRepository.AddMessageAsync(msg);
            await _conversationService.UpdateLastMessagePreviewAsync(conversationId, content);

            return await _messageRepository.GetMessageByIdAsync(savedMessage.Id) ?? savedMessage;
        }

        public async Task<Message> SendAttachmentMessageAsync(int conversationId, int senderId, string fileName, string fileUrl, long fileSize, MessageType type)
        {
            var msg = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Type = type,
                Content = null, // text is null for pure attachment message
                CreatedAt = DateTime.UtcNow.AddHours(7),
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        FileName = fileName,
                        StoredFileName = fileName,
                        FileUrl = fileUrl,
                        SizeBytes = fileSize,
                        MimeType = type == MessageType.Image ? "image/jpeg" : "application/octet-stream",
                        Type = type == MessageType.Image ? AttachmentType.Image : AttachmentType.File,
                        UploadedAt = DateTime.UtcNow.AddHours(7)
                    }
                }
            };

            var savedMessage = await _messageRepository.AddMessageAsync(msg);
            
            var preview = type == MessageType.Image ? "[Image]" : "[File]";
            await _conversationService.UpdateLastMessagePreviewAsync(conversationId, preview);

            return await _messageRepository.GetMessageByIdAsync(savedMessage.Id) ?? savedMessage;
        }

        public async Task MarkAsReadAsync(int messageId, int userId)
        {
            await _messageRepository.MarkMessageAsReadAsync(messageId, userId);
        }
    }
}