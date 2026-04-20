using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Chat_Group_System.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userIdStr = httpContext?.Request.Query["userId"];
            
            if (!string.IsNullOrEmpty(userIdStr))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userIdStr}");
            }
            
            await base.OnConnectedAsync();
        }

        public async Task JoinGroup(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task LeaveGroup(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task SendMessage(int conversationId, int senderId, string content)
        {
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", conversationId, senderId, content);
        }

        public async Task UserTyping(int conversationId, string userName, bool isTyping)
        {
            await Clients.OthersInGroup(conversationId.ToString()).SendAsync("UserTyping", conversationId, userName, isTyping);
        }

        public async Task NotifyAddedToGroup(int targetUserId, int conversationId)
        {
            await Clients.Group($"User_{targetUserId}").SendAsync("AddedToGroup", conversationId);
        }
    }
}
