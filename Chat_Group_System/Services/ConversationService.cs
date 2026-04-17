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
                CreatedAt = DateTime.UtcNow.AddHours(7)
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
                CreatedAt = DateTime.UtcNow.AddHours(7)
            };

            return await _conversationRepository.AddAsync(conversation, new[] { user1Id, user2Id });
        }

        public async Task UpdateLastMessagePreviewAsync(int conversationId, string previewText)
        {
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation != null)
            {
                conversation.LastMessagePreview = previewText;
                conversation.LastMessageAt = DateTime.UtcNow.AddHours(7);
                await _conversationRepository.UpdateAsync(conversation);
            }
        }

        public async Task AddMemberToGroupAsync(int conversationId, int currentUserId, int newMemberId)
        {
            var members = await _conversationRepository.GetMembersAsync(conversationId);
            var currentUserMember = members.FirstOrDefault(m => m.UserId == currentUserId);
            
            if (currentUserMember == null || currentUserMember.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Only Admins can add members.");
            }

            if (members.Any(m => m.UserId == newMemberId))
            {
                throw new InvalidOperationException("User is already a member.");
            }

            await _conversationRepository.AddMemberAsync(conversationId, newMemberId, "Member");
        }

        public async Task RemoveMemberFromGroupAsync(int conversationId, int currentUserId, int memberToRemoveId)
        {
            var members = await _conversationRepository.GetMembersAsync(conversationId);
            var currentUserMember = members.FirstOrDefault(m => m.UserId == currentUserId);

            if (currentUserMember == null || currentUserMember.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Only Admins can remove members.");
            }

            if (currentUserId == memberToRemoveId)
            {
                throw new InvalidOperationException("Use LeaveGroup to leave the conversation.");
            }

            await _conversationRepository.RemoveMemberAsync(conversationId, memberToRemoveId);
        }

        public async Task LeaveOrDisbandGroupAsync(int conversationId, int currentUserId)
        {
            var members = await _conversationRepository.GetMembersAsync(conversationId);
            var currentUserMember = members.FirstOrDefault(m => m.UserId == currentUserId);

            if (currentUserMember == null)
            {
                throw new InvalidOperationException("User is not a member of this conversation.");
            }

            if (currentUserMember.Role == "Admin")
            {
                await _conversationRepository.DeleteConversationAsync(conversationId);
            }
            else
            {
                await _conversationRepository.RemoveMemberAsync(conversationId, currentUserId);
            }
        }

        public async Task<IEnumerable<ConversationMember>> GetGroupMembersAsync(int conversationId)
        {
            return await _conversationRepository.GetMembersAsync(conversationId);
        }
    }
}