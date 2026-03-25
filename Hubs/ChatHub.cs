using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartInsuranceHub.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{role}_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string receiverId, string senderType, string message)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(senderRole)) return;

            string receiverRole = senderRole == "Customer" ? "Agent" : "Customer";
            
            // Broadcast to the specific user via their targeted Group
            await Clients.Group($"{receiverRole}_{receiverId}").SendAsync("ReceiveMessage", senderId, senderRole, message, DateTime.UtcNow.ToString("o"));
        }
    }
}
