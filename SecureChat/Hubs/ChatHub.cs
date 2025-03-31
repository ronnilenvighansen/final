using Microsoft.AspNetCore.SignalR;

namespace SecureChat.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string receiver, string encryptedMessage)
        {
            await Clients.User(receiver).SendAsync("ReceiveMessage", Context.UserIdentifier, encryptedMessage);
        }
    }
}
