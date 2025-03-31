using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SecureChat.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();

        public async Task RegisterUser(string username)
        {
            _userConnections[username] = Context.ConnectionId;
            await Clients.Caller.SendAsync("RegistrationSuccess", $"Registered as {username}");
        }

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;
            await Clients.Caller.SendAsync("ReceiveConnectionId", connectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (user != null)
            {
                _userConnections.TryRemove(user, out _);
            }            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string receiverUsername, string encryptedMessage)
        {
            if (_userConnections.TryGetValue(receiverUsername, out string? targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveMessage", Context.ConnectionId, encryptedMessage);
            }
        }
    }
}
