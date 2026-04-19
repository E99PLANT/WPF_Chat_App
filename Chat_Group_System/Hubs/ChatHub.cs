using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Chat_Group_System.Hubs
{
    public class ChatHub : Hub
    {
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

        public async Task UserTyping(int conversationId, string userName)
        {
            await Clients.OthersInGroup(conversationId.ToString()).SendAsync("UserTyping", conversationId, userName);
        }
    }
}
