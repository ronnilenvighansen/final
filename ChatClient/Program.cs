using Microsoft.AspNetCore.SignalR.Client;
using ChatClient.Security;

class Program
{
    private static HubConnection _connection;
    private static string _username = "";

    static async Task Main()
    {
        Console.Write("Enter your username and then press Enter: ");
        _username = Console.ReadLine() ?? "User";
        string myPublicKey = RSAEncryption.GetPublicKey();

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7151/chatHub")
            .Build();

        _connection.On<string>("RegistrationSuccess", (message) =>
        {
            Console.WriteLine(message);
        });

        _connection.On<string, string, string, string, string>("ReceiveMessage", (sender, encryptedMessage, encryptedKey, iv, hmac) =>
        {
            Console.WriteLine($"\nEncrypted message:\n{sender}: {encryptedMessage}");

            try
            {
                string decryptedAesKey = RSAEncryption.DecryptWithPrivateKey(encryptedKey);
                AesHmacEncryption.SetKey(decryptedAesKey);
                string decryptedMessage = AesHmacEncryption.Decrypt(encryptedMessage, iv, hmac);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nDecrypted message:\n{sender}: {decryptedMessage}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Decryption failed: {ex.Message}");
                Console.ResetColor();
            }
        });

        try
        {
            await _connection.StartAsync();
            Console.WriteLine("Connected to chat.");
            await _connection.InvokeAsync("RegisterUser", _username, myPublicKey);
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
            Console.Write("Enter receiver username and then press Enter (Write Exit and press Enter to exit): ");
            var receiver = Console.ReadLine();

            if (receiver.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

            if (string.IsNullOrWhiteSpace(receiver)) continue;

            Console.Write("Write a message and then press Enter (Write Exit and press Enter to exit): ");
            var message = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(message)) continue;
            if (message.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
            try
            {
                string receiverPublicKey = await _connection.InvokeAsync<string>("GetPublicKey", receiver);

                if (string.IsNullOrEmpty(receiverPublicKey))
                {
                    Console.WriteLine($"Could not retrieve public key for {receiver}");
                    continue;
                }

                string aesKey = AesHmacEncryption.GenerateRandomKey();
                AesHmacEncryption.SetKey(aesKey);
                var (encryptedMessage, iv, hmac) = AesHmacEncryption.Encrypt(message);

                string encryptedAesKey = RSAEncryption.EncryptWithPublicKey(aesKey, receiverPublicKey);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("(Sending...)");
                Console.ResetColor();
                
                await _connection.InvokeAsync("SendMessage", receiver, encryptedMessage, encryptedAesKey, iv, hmac);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send message: {ex.Message}");
            }
        }

        await _connection.StopAsync();
    }
}
