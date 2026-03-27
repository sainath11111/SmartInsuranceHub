using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace SmartInsuranceHub.Hubs
{
    public class ChatHub : Hub
    {
        // Tracks Agent Connections by agent_id
        public static readonly ConcurrentDictionary<int, int> OnlineAgents = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"{role}_{userId}");
                
                if (role == "Agent" && int.TryParse(userId, out int id))
                {
                    var newCount = OnlineAgents.AddOrUpdate(id, 1, (_, count) => count + 1);
                    if (newCount == 1) // Just came online
                        await Clients.All.SendAsync("AgentOnlineStatus", id, true);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Agent" && int.TryParse(userId, out int id))
            {
                if (OnlineAgents.TryGetValue(id, out int count))
                {
                    if (count <= 1)
                    {
                        OnlineAgents.TryRemove(id, out _);
                        await Clients.All.SendAsync("AgentOnlineStatus", id, false);
                    }
                    else
                    {
                        OnlineAgents.TryUpdate(id, count - 1, count);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
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
