using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Chat_Group_System.Models.Entities;
using Chat_Group_System.Services;
using Microsoft.Extensions.Configuration;

namespace Chat_Group_System.Controllers
{
    public class ChatController
    {
        private readonly IMessageService _messageService;
        private readonly IConversationService _conversationService;
        private readonly ISignalRService _signalRService;
        private readonly IConfiguration _configuration;

        // Bọc lại các Event của SignalR để UI (MainWindow) lắng nghe
        public event Action<int, int, string>? OnMessageReceived;
        public event Action<int, string, bool>? OnUserTyping;
        public event Action<int, bool>? OnUserOnlineStatusChanged;
        public event Action<int>? OnAddedToGroup;

        public ChatController(
            IMessageService messageService, 
            IConversationService conversationService, 
            ISignalRService signalRService,
            IConfiguration configuration)
        {
            _messageService = messageService;
            _conversationService = conversationService;
            _signalRService = signalRService;
            _configuration = configuration;

            // Chuyển tiếp event từ Service lên Controller
            _signalRService.MessageReceived += (convId, senderId, content) => OnMessageReceived?.Invoke(convId, senderId, content);
            _signalRService.UserTyping += (convId, userName, isTyping) => OnUserTyping?.Invoke(convId, userName, isTyping);
            _signalRService.UserOnlineStatusChanged += (uid, isOnline) => OnUserOnlineStatusChanged?.Invoke(uid, isOnline);
            _signalRService.AddedToGroup += (convId) => OnAddedToGroup?.Invoke(convId);
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

        public async Task JoinGroupAsync(int conversationId)
        {
            await _signalRService.JoinGroupAsync(conversationId);
        }

        public async Task LeaveGroupAsync(int conversationId)
        {
            await _signalRService.LeaveGroupAsync(conversationId);
        }

        public async Task NotifyTypingAsync(int conversationId, string userName, bool isTyping)
        {
            await _signalRService.NotifyTypingAsync(conversationId, userName, isTyping);
        }

        public async Task NotifyUserAddedToGroupAsync(int targetUserId, int conversationId)
        {
            await _signalRService.NotifyUserAddedToGroupAsync(targetUserId, conversationId);
        }

        // ── Conversations ──────────────────────────────────────
        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _conversationService.GetUserConversationsAsync(userId);
        }

        public async Task<(bool Success, string Message, Conversation? Conversation)> CreateOrGetDirectMessageAsync(int user1Id, int user2Id)
        {
            try
            {
                var dm = await _conversationService.CreateOrGetDirectMessageAsync(user1Id, user2Id);
                return (true, "DM retrieved successfully", dm);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to get or create direct message: {ex.Message}", null);
            }
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
                
                // Fetch the new user's name to display in the system message
                var newUser = await _conversationService.GetGroupMembersAsync(conversationId);
                var newMemberName = newUser.FirstOrDefault(u => u.UserId == newMemberId)?.User?.DisplayName ?? "Người dùng mới";

                var sysMsg = await _messageService.SendSystemMessageAsync(conversationId, $"{newMemberName} đã tham gia nhóm.");
                // Broadcast system message
                var payload = System.Text.Json.JsonSerializer.Serialize(new { 
                    Id = sysMsg.Id,
                    Type = MessageType.System,
                    Content = sysMsg.Content
                });
                await _signalRService.SendMessageAsync(conversationId, sysMsg.SenderId, "[SYSTEM]" + payload);

                // Notify the added user so they can join realtime group
                await NotifyUserAddedToGroupAsync(newMemberId, conversationId);

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
                // Fetch name before removing 
                var membersInfo = await _conversationService.GetGroupMembersAsync(conversationId);
                var memberName = membersInfo.FirstOrDefault(u => u.UserId == memberToRemoveId)?.User?.DisplayName ?? "Người dùng";

                await _conversationService.RemoveMemberFromGroupAsync(conversationId, currentUserId, memberToRemoveId);

                var sysMsg = await _messageService.SendSystemMessageAsync(conversationId, $"{memberName} đã bị xoá khỏi nhóm.");
                var payload = System.Text.Json.JsonSerializer.Serialize(new { 
                    Id = sysMsg.Id,
                    Type = MessageType.System,
                    Content = sysMsg.Content
                });
                await _signalRService.SendMessageAsync(conversationId, sysMsg.SenderId, "[SYSTEM]" + payload);

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
                // Fetch name before leaving
                var membersInfo = await _conversationService.GetGroupMembersAsync(conversationId);
                var memberName = membersInfo.FirstOrDefault(u => u.UserId == currentUserId)?.User?.DisplayName ?? "Người dùng";

                await _conversationService.LeaveOrDisbandGroupAsync(conversationId, currentUserId);

                // Send system message only if the group wasn't disbanded completely
                // (e.g. if IConversationRepository.GetGroupById returns not null, but since we are just adding a message, it might be fine)
                
                var sysMsg = await _messageService.SendSystemMessageAsync(conversationId, $"{memberName} đã rời khỏi nhóm.");
                var payload = System.Text.Json.JsonSerializer.Serialize(new { 
                    Id = sysMsg.Id,
                    Type = MessageType.System,
                    Content = sysMsg.Content
                });
                await _signalRService.SendMessageAsync(conversationId, sysMsg.SenderId, "[SYSTEM]" + payload);

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
            string uploadsFolder = _configuration["AppSettings:UploadFolder"] ?? @"\\10.87.11.53\FIleUploads";
            try
            {
                string fileName = Path.GetFileName(filePath);

                if (!Path.IsPathRooted(uploadsFolder))
                {
                    uploadsFolder = Path.GetFullPath(uploadsFolder);
                }

                // Nếu là đường dẫn mạng, kiểm tra sự tồn tại trước khi Create
                if (!Directory.Exists(uploadsFolder))
                {
                    try {
                        Directory.CreateDirectory(uploadsFolder);
                    } catch (Exception exDir) {
                        throw new Exception($"Cannot access or create folder: {uploadsFolder}. System error: {exDir.Message}");
                    }
                }

                string storedFileName = $"{Guid.NewGuid()}_{fileName}";
                string storedFilePath = Path.Combine(uploadsFolder, storedFileName);
                File.Copy(filePath, storedFilePath, true);

                long fileSize = new FileInfo(storedFilePath).Length;

                // 1. Lưu vào Database
                var savedMsg = await _messageService.SendAttachmentMessageAsync(conversationId, senderId, fileName, storedFilePath, fileSize, type);

                // 2. Broadcast JSON payload using the new centralized FileUrl
                string finalFilePath = savedMsg.Attachments?.FirstOrDefault()?.FileUrl ?? filePath;
                var payload = System.Text.Json.JsonSerializer.Serialize(new {
                    Id = savedMsg.Id,
                    Type = type,
                    FileName = fileName,
                    FileSize = fileSize,
                    FilePath = finalFilePath
                });
                await _signalRService.SendMessageAsync(conversationId, senderId, "[ATTACHMENT]" + payload);

                return (true, "Attachment sent successfully", savedMsg);
            }
            catch (Exception ex)
            {
                string errorMsg = $"Failed to send attachment.\nPath: {uploadsFolder}\nError: {ex.Message}";
                if (ex.InnerException != null) errorMsg += $"\nInner: {ex.InnerException.Message}";
                return (false, errorMsg, null);
            }
        }
    }
}