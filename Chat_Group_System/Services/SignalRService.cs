using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace Chat_Group_System.Services
{
    public class SignalRService : ISignalRService
    {
        private HubConnection? _hubConnection;
        private readonly string _hubUrlSettings;

        public event Action<int, int, string>? MessageReceived;
        public event Action<int, string>? UserTyping;
        public event Action<int, bool>? UserOnlineStatusChanged;

        public SignalRService(IConfiguration config)
        {
            _hubUrlSettings = config["SignalR:HubUrl"] ?? "http://localhost:5000/chatHub";
        }

        public async Task ConnectAsync(int userId)
        {
            var hubUrl = $"{_hubUrlSettings}?userId={userId}";

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
                try 
                {
                    await _hubConnection.StopAsync();
                }
                catch { /* Ignore if already stopped */ }
                
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }

        public async Task SendMessageAsync(int conversationId, int senderId, string content)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendMessage", conversationId, senderId, content);
            }
        }

        public async Task JoinGroupAsync(int conversationId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("JoinGroup", conversationId.ToString());
            }
        }

        public async Task LeaveGroupAsync(int conversationId)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveGroup", conversationId.ToString());
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