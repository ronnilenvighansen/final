using Microsoft.AspNetCore.SignalR.Client;

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

        _connection.On<string, string>("ReceiveMessage", (sender, message) =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{sender}: {message}");
            Console.ResetColor();
        });

        try
        {
            await _connection.StartAsync();
            await _connection.InvokeAsync("RegisterUser", _username);
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
        Console.Write("Enter recipient's username: ");
        var recipientUsername = Console.ReadLine();
        
        while (true)
        {
            Console.Write("You: ");
            var message = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(message)) continue;
            if (message.Equals("/exit", StringComparison.OrdinalIgnoreCase)) break;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("(Sending...)");
            Console.ResetColor();

            await _connection.InvokeAsync("SendMessage", recipientUsername, message);
        }

        await _connection.StopAsync();
    }
}
