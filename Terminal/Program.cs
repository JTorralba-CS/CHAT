using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Standard.Models;
using Standard.Services;
using System.Text;

namespace Terminal
{
    public class Program
    {
        private static IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static ChatService Chat = new ChatService(configuration["ChatHub"]);

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Chat.HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
            {
                //Console.WriteLine();
                //Console.WriteLine($"Chat.cs ReceiveMessage {Chat.HubConnection.ConnectionId} {connection.ID} {connection.Alias} \"{message}\"");

                if (connection.ID == Chat.HubConnection.ConnectionId || connection.Alias == Chat.Connection.Alias)
                {
                    Log($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                }
                else
                {
                    Log($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                }

                Console.ForegroundColor = ConsoleColor.White;
            });

            Chat.Send(". Terminal");

            while (true)
            {
                var message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    break;
                }

                Chat.Send(message);
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
