using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat_Group_System.Services
{
    public interface ISignalRService
    {
        event Action<int, int, string>? MessageReceived;
        event Action<int, string>? UserTyping;
        event Action<int, bool>? UserOnlineStatusChanged;

        Task ConnectAsync(int userId);
        Task DisconnectAsync();
        Task JoinGroupAsync(int conversationId);
        Task LeaveGroupAsync(int conversationId);
        Task SendMessageAsync(int conversationId, int senderId, string content);
        Task NotifyTypingAsync(int conversationId, string userName);
    }
}