using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Chat_Group_System.Models.Data;
using Chat_Group_System.Models.Entities;

namespace Chat_Group_System.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly NexChatDbContext _context;

        public ConversationRepository(NexChatDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _context.Conversations
                .Where(c => c.Members.Any(m => m.UserId == userId))
                .Include(c => c.Members)
                .ThenInclude(m => m.User)
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Conversation?> GetByIdAsync(int id)
        {
            return await _context.Conversations
                .Include(c => c.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Conversation?> GetDirectMessageAsync(int userId1, int userId2)
        {
            return await _context.Conversations
                .Where(c => c.Type == ConversationType.DirectMessage && c.Members.Count == 2)
                .FirstOrDefaultAsync(c =>
                    c.Members.Any(m => m.UserId == userId1) &&
                    c.Members.Any(m => m.UserId == userId2));
        }

        public async Task<Conversation> AddAsync(Conversation conversation, IEnumerable<int> participantIds, int creatorId = 0)
        {
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();

            var members = participantIds.Select(id => new ConversationMember
            {
                ConversationId = conversation.Id,
                UserId = id,
                JoinedAt = Chat_Group_System.Helpers.TimeHelper.NowVN,
                Role = (creatorId > 0 && id == creatorId) ? "Admin" : "Member" 
            });

            await _context.ConversationMembers.AddRangeAsync(members);
            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task UpdateAsync(Conversation conversation)
        {
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task AddMemberAsync(int conversationId, int userId, string role)
        {
            var member = new ConversationMember
            {
                ConversationId = conversationId,
                UserId = userId,
                Role = role,
                JoinedAt = Chat_Group_System.Helpers.TimeHelper.NowVN
            };
            await _context.ConversationMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveMemberAsync(int conversationId, int userId)
        {
            var member = await _context.ConversationMembers
                .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.UserId == userId);
            if (member != null)
            {
                _context.ConversationMembers.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteConversationAsync(int conversationId)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation != null)
            {
                _context.Conversations.Remove(conversation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ConversationMember>> GetMembersAsync(int conversationId)
        {
            return await _context.ConversationMembers
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.User)
                .ToListAsync();
        }
    }
}
