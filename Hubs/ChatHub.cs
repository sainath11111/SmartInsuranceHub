using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartInsuranceHub.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string receiverId, string senderType, string message)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId)) return;

            // Broadcast to the specific user via their connected UserIdentifier
            // We use the connected user's ID as the group/user name in SignalR
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, senderType, message, DateTime.UtcNow.ToString("o"));
        }
    }
}
