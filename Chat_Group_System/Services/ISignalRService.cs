using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat_Group_System.Services
{
    public interface ISignalRService
    {
        event Action<int, int, string>? MessageReceived;
        event Action<int, string, bool>? UserTyping; // Updated with bool isTyping
        event Action<int, bool>? UserOnlineStatusChanged;
        event Action<int>? AddedToGroup; // New event for generic group notification

        Task ConnectAsync(int userId);
        Task DisconnectAsync();
        Task JoinGroupAsync(int conversationId);
        Task LeaveGroupAsync(int conversationId);
        Task SendMessageAsync(int conversationId, int senderId, string content);
        Task NotifyTypingAsync(int conversationId, string userName, bool isTyping);
        Task NotifyUserAddedToGroupAsync(int targetUserId, int conversationId);
    }
}