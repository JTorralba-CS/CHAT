
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

using Standard.Models;

namespace Terminal.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public ChatService() : base(Configuration["ChatHub"])
        {
            Console.Title = "Terminal";

            Connection.Alias = "Terminal";

            HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
            {
                //Console.WriteLine();
                //Console.WriteLine($"Terminal Program.cs ReceiveMessage {Chat.HubConnection.ConnectionId} {connection.ID} {connection.Alias} \"{message}\"");

                if (connection.ID == HubConnection.ConnectionId || connection.Alias == Connection.Alias)
                {
                    Log($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                }
                else
                {
                    Log($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                }

                Console.ForegroundColor = ConsoleColor.White;
            });
        }

        public static void Log(string message, ConsoleColor consoleColor = ConsoleColor.White)
        {
            Console.ForegroundColor = consoleColor;

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {message}");

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
