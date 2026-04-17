using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Chat_Group_System.Models.Entities;
using Chat_Group_System.Services;

namespace Chat_Group_System.Controllers
{
    public class ChatController
    {
        private readonly IMessageService _messageService;
        private readonly IConversationService _conversationService;
        private readonly ISignalRService _signalRService;

        // Bọc lại các Event của SignalR để UI (MainWindow) lắng nghe
        public event Action<int, int, string>? OnMessageReceived;
        public event Action<int, string>? OnUserTyping;
        public event Action<int, bool>? OnUserOnlineStatusChanged;

        public ChatController(
            IMessageService messageService, 
            IConversationService conversationService, 
            ISignalRService signalRService)
        {
            _messageService = messageService;
            _conversationService = conversationService;
            _signalRService = signalRService;

            // Chuyển tiếp event từ Service lên Controller
            _signalRService.MessageReceived += (convId, senderId, content) => OnMessageReceived?.Invoke(convId, senderId, content);
            _signalRService.UserTyping += (convId, userName) => OnUserTyping?.Invoke(convId, userName);
            _signalRService.UserOnlineStatusChanged += (uid, isOnline) => OnUserOnlineStatusChanged?.Invoke(uid, isOnline);
        }

        // ── SignalR Connection ─────────────────────────────────
        public async Task ConnectRealtimeAsync(int userId)
        {
            await _signalRService.ConnectAsync(userId);
        }

        public async Task DisconnectRealtimeAsync()
        {
            await _signalRService.DisconnectAsync();
        }

        // ── Conversations ──────────────────────────────────────
        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _conversationService.GetUserConversationsAsync(userId);
        }

        public async Task<(bool Success, string Message, Conversation? Group)> CreateGroupAsync(int creatorId, string groupName, IEnumerable<int> memberIds)
        {
            try
            {
                var group = await _conversationService.CreateGroupChatAsync(creatorId, groupName, memberIds);
                // Could broadcast a signal to users to refresh list
                return (true, "Group created successfully", group);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create group: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> AddMemberToGroupAsync(int conversationId, int currentUserId, int newMemberId)
        {
            try
            {
                await _conversationService.AddMemberToGroupAsync(conversationId, currentUserId, newMemberId);
                return (true, "Member added successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> RemoveMemberFromGroupAsync(int conversationId, int currentUserId, int memberToRemoveId)
        {
            try
            {
                await _conversationService.RemoveMemberFromGroupAsync(conversationId, currentUserId, memberToRemoveId);
                return (true, "Member removed successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> LeaveOrDisbandGroupAsync(int conversationId, int currentUserId)
        {
            try
            {
                await _conversationService.LeaveOrDisbandGroupAsync(conversationId, currentUserId);
                return (true, "Left/Disbanded successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<IEnumerable<ConversationMember>> GetGroupMembersAsync(int conversationId)
        {
            return await _conversationService.GetGroupMembersAsync(conversationId);
        }

        // ── Messages ───────────────────────────────────────────
        public async Task<IEnumerable<Message>> GetRecentMessagesAsync(int conversationId)
        {
            return await _messageService.GetRecentMessagesAsync(conversationId);
        }

        public async Task<(bool Success, string Message, Message? SentMessage)> SendTextMessageAsync(int conversationId, int senderId, string content)
        {
            try
            {
                // 1. Lưu vào Database thông qua Service -> Repo
                var savedMsg = await _messageService.SendTextMessageAsync(conversationId, senderId, content);
                
                // 2. Bắn SignalR broadcast
                await _signalRService.SendMessageAsync(conversationId, senderId, content);
                
                return (true, "Sent successfully", savedMsg);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send message: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, Message? SentMessage)> SendAttachmentMessageAsync(int conversationId, int senderId, string filePath, MessageType type)
        {
            try
            {
                // Thực tế: Cần đẩy file lên Server/Cloud/S3 và lấy URL về.
                // Ở đây mô phỏng lấy tên file và dùng đường dẫn file ở local.
                string fileName = System.IO.Path.GetFileName(filePath);
                long fileSize = new System.IO.FileInfo(filePath).Length;

                // 1. Lưu vào Database
                var savedMsg = await _messageService.SendAttachmentMessageAsync(conversationId, senderId, fileName, filePath, fileSize, type);

                // 2. Broadcast (Sử dụng SignalR, gửi nội dung báo có file)
                await _signalRService.SendMessageAsync(conversationId, senderId, "Sent an attachment: " + fileName);

                return (true, "Attachment sent successfully", savedMsg);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send attachment: {ex.Message}", null);
            }
        }
    }
}