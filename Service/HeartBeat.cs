using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text;
using System.Timers;

using Standard.Models;
using Standard.Services;

namespace Service
{
    public class HeartBeat
    {
        private ChatService Chat = new ChatService();

        private readonly Timer timer;

        private const int heartbeatInterval = 17;

        private static bool DEBUG = false;

        public HeartBeat()
        {
            if (DEBUG)
            {
                Console.OutputEncoding = Encoding.UTF8;
            }

            try
            {
                Chat.HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
                {
                    if (DEBUG)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Chat.cs ReceiveMessage {Chat.HubConnection.ConnectionId} {connection.ID} {connection.Alias} \"{message}\"");
                    }

                    if (connection.ID == Chat.HubConnection.ConnectionId || connection.Alias == Chat.Connection.Alias)
                    {
                        Log($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        Log($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                    }
                });

                Chat.Send(". Service");
            }
            catch (Exception e)
            {
                if (DEBUG)
                {
                    Console.WriteLine($"Serivce HeartBeat.cs HeartBeat(): {e.Message}");
                }
            }

            timer = new Timer(heartbeatInterval * 1000) { AutoReset = true };
            timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs a)
        {
            Chat.Send("❤");
        }

        public static void Log(string message, ConsoleColor consoleColor = ConsoleColor.White)
        {
            if (DEBUG)
            {
                Console.ForegroundColor = consoleColor;

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {message}");

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
