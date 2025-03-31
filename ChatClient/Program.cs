using Microsoft.AspNetCore.SignalR.Client;

class Program
{
    private static HubConnection _connection;
    private static string _username = "";

    static async Task Main()
    {
        Console.Write("Enter your username: ");
        _username = Console.ReadLine() ?? "User";

        // Setup connection to SignalR hub
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5097/chatHub")
            .Build();

        _connection.On<string, string>("ReceiveMessage", (sender, message) =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{sender}: {message}");
            Console.ResetColor();
        });

        try
        {
            await _connection.StartAsync();
            Console.WriteLine("Connected to chat.");
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
            Console.Write("You: ");
            var message = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(message)) continue;
            if (message.Equals("/exit", StringComparison.OrdinalIgnoreCase)) break;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("(Sending...)");
            Console.ResetColor();

            await _connection.InvokeAsync("SendMessage", "Receiver", message);
        }

        await _connection.StopAsync();
    }
}
