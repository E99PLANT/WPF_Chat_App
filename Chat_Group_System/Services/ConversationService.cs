using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;
using Chat_Group_System.Repositories;

namespace Chat_Group_System.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _conversationRepository;

        public ConversationService(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _conversationRepository.GetUserConversationsAsync(userId);
        }

        public async Task<Conversation?> GetConversationByIdAsync(int conversationId)
        {
            return await _conversationRepository.GetByIdAsync(conversationId);
        }

        public async Task<Conversation> CreateGroupChatAsync(int creatorId, string groupName, IEnumerable<int> memberIds)
        {
            var conversation = new Conversation
            {
                Name = groupName,
                Type = ConversationType.Group,
                CreatedAt = DateTime.UtcNow
            };

            var allMembers = memberIds.ToList();
            if (!allMembers.Contains(creatorId))
            {
                allMembers.Add(creatorId);
            }

            return await _conversationRepository.AddAsync(conversation, allMembers);
        }

        public async Task<Conversation> CreateOrGetDirectMessageAsync(int user1Id, int user2Id)
        {
            var existingDm = await _conversationRepository.GetDirectMessageAsync(user1Id, user2Id);
            if (existingDm != null)
            {
                return existingDm;
            }

            var conversation = new Conversation
            {
                Name = null, // DM has no explicitly set name, derived from user
                Type = ConversationType.DirectMessage,
                CreatedAt = DateTime.UtcNow
            };

            return await _conversationRepository.AddAsync(conversation, new[] { user1Id, user2Id });
        }

        public async Task UpdateLastMessagePreviewAsync(int conversationId, string previewText)
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation != null)
            {
                conversation.LastMessagePreview = previewText;
                conversation.LastMessageAt = DateTime.UtcNow;
                await _conversationRepository.UpdateAsync(conversation);
            }
        }
    }
}