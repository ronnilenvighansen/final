using Microsoft.AspNetCore.SignalR.Client;
using SharedSecurity;

class Program
{
    private static HubConnection _connection;
    private static string _username = "";

    static async Task Main()
    {
        Console.Write("Enter your username: ");
        _username = Console.ReadLine() ?? "User";

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5097/chatHub")
            .Build();

        _connection.On<string>("RegistrationSuccess", (message) =>
        {
            Console.WriteLine(message);
        });

        _connection.On<string, string>("ReceiveMessage", (sender, encryptedMessage) =>
        {
            Console.WriteLine($"\nEncrypted message:\n{sender}: {encryptedMessage}");
            string decryptedMessage = AesEncryption.Decrypt(encryptedMessage);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{sender}: {decryptedMessage}");
            Console.ResetColor();
        });

        try
        {
            await _connection.StartAsync();
            Console.WriteLine("Connected to chat.");
            await _connection.InvokeAsync("RegisterUser", _username);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect: {ex.Message}");
            return;
        }

        await MessageLoop();
    }

    static async Task MessageLoop()
    {   
        while (true)
        {
            Console.Write("Enter receiver username: ");
            var receiver = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(receiver)) continue;

            Console.Write("You: ");
            var message = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(message)) continue;
            if (message.Equals("/exit", StringComparison.OrdinalIgnoreCase)) break;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("(Sending...)");
            Console.ResetColor();
            try
            {
                await _connection.InvokeAsync("SendMessage", receiver, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message: {ex.Message}");
            }
        }

        await _connection.StopAsync();
    }
}
