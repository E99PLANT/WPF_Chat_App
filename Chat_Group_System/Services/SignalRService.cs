using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat_Group_System.Services
{
    public class SignalRService : ISignalRService
    {
        private HubConnection? _hubConnection;

        public event Action<int, int, string>? MessageReceived;
        public event Action<int, string>? UserTyping;
        public event Action<int, bool>? UserOnlineStatusChanged;

        public async Task ConnectAsync(int userId)
        {
            // Update this URL to match your actual SignalR server URL
            var hubUrl = $"https://localhost:5001/chatHub?userId={userId}";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<int, int, string>("ReceiveMessage", (convId, senderId, content) =>
            {
                MessageReceived?.Invoke(convId, senderId, content);
            });

            _hubConnection.On<int, string>("UserTyping", (convId, userName) =>
            {
                UserTyping?.Invoke(convId, userName);
            });

            _hubConnection.On<int, bool>("UserOnlineStatus", (uid, isOnline) =>
            {
                UserOnlineStatusChanged?.Invoke(uid, isOnline);
            });

            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error connecting SignalR: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
        }

        public async Task SendMessageAsync(int conversationId, int senderId, string content)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendMessage", conversationId, senderId, content);
            }
        }

        public async Task NotifyTypingAsync(int conversationId, string userName)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("UserTyping", conversationId, userName);
            }
        }
    }
}