using Microsoft.AspNetCore.SignalR.Client;

using Standard.Models;
using Standard.Services;

namespace Terminal
{
    public class Program
    {
        public static ChatService Chat = new ChatService();

        static void Main(string[] args)
        {
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
