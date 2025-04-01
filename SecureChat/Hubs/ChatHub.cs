using Microsoft.AspNetCore.SignalR;
using SharedSecurity;
using System.Collections.Concurrent;

namespace SecureChat.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();

        public async Task RegisterUser(string username)
        {
            _userConnections[username] = Context.ConnectionId;
            Console.WriteLine($"✅ User '{username}' registered with Connection ID: {Context.ConnectionId}");
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

        public async Task SendMessage(string receiverUsername, string message)
        {
            try
            {
                if (_userConnections.TryGetValue(receiverUsername, out string? targetConnectionId))
                {
                    var senderUsername = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
                    
                    if (string.IsNullOrEmpty(senderUsername))
                    {
                        Console.WriteLine("Sender username not found.");
                        return;
                    }
                    
                    Console.WriteLine($"Sending message from Username: {senderUsername} to Username: {receiverUsername}");
                    string encryptedMessage = AesEncryption.Encrypt(message);
                    await Clients.Client(targetConnectionId).SendAsync("ReceiveMessage", senderUsername, encryptedMessage);
                }
                else
                {
                    Console.WriteLine($"User {receiverUsername} not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendMessage: {ex.Message}");
                throw;
            }
        }
    }
}
