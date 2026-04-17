using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Chat_Group_System.Models.Data;
using Chat_Group_System.Models.Entities;

namespace Chat_Group_System.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly NexChatDbContext _context;

        public MessageRepository(NexChatDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Message>> GetMessagesByConversationIdAsync(int conversationId, int skip = 0, int take = 50)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .Include(m => m.Sender)
                .Include(m => m.Attachments)
                .Include(m => m.ReadReceipts)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Message> AddMessageAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task MarkMessageAsReadAsync(int messageId, int userId)
        {
            var alreadyRead = await _context.MessageReads
                .AnyAsync(r => r.MessageId == messageId && r.UserId == userId);

            if (!alreadyRead)
            {
                await _context.MessageReads.AddAsync(new MessageRead
                {
                    MessageId = messageId,
                    UserId = userId,
                    ReadAt = DateTime.UtcNow.AddHours(7)
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Message?> GetMessageByIdAsync(int id)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}