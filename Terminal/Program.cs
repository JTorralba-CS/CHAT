using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Standard.Models;
using Standard.Services;

namespace Terminal
{
    public class Program
    {
        private static IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static Standard.Services.ChatService ChatService = new ChatService(configuration["ChatHub"]);

        static void Main(string[] args)
        {
            Console.Title = "Terminal";

            ChatService.Connection.Alias = "Terminal";

            ChatService.HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
            {
                //Console.WriteLine();
                //Console.WriteLine($"Terminal Program.cs ReceiveMessage {Chat.HubConnection.ConnectionId} {connection.ID} {connection.Alias} \"{message}\"");

                if (connection.ID == ChatService.HubConnection.ConnectionId || connection.Alias == ChatService.Connection.Alias)
                {
                    Log($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                }
                else
                {
                    Log($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                }

                Console.ForegroundColor = ConsoleColor.White;
            });

            while (true)
            {
                var message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    break;
                }

                ChatService.Send(message);
            }

            Console.ReadLine();
        }

        public static void Log(string message, ConsoleColor consoleColor = ConsoleColor.White)
        {
            Console.ForegroundColor = consoleColor;

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {message}");
                
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
