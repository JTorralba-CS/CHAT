using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Timers;

using Standard.Models;
using Standard.Services;
using Microsoft.Extensions.Configuration;

namespace Service
{
    public class HeartBeat
    {
        private static IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        private Standard.Services.ChatService ChatService = new ChatService(configuration["ChatHub"]);

        private readonly Timer timer;

        private const int heartbeatInterval = 60;

        private static bool debug = true;

        public HeartBeat()
        {
            try
            {
                ChatService.Connection.Alias = "Service";

                ChatService.HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
                {
                    if (debug)
                    {
                        //Console.WriteLine();
                        //Console.WriteLine($"Service HeartBeat.cs ReceiveMessage {Chat.HubConnection.ConnectionId} {connection.ID} {connection.Alias} \"{message}\"");
                    }

                    if (connection.ID == ChatService.HubConnection.ConnectionId || connection.Alias == ChatService.Connection.Alias)
                    {
                        Log($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        Log($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                    }
                });
            }
            catch (Exception e)
            {
                if (debug)
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
            ChatService.Send("❤");
        }

        public static void Log(string message, ConsoleColor consoleColor = ConsoleColor.White)
        {
            if (debug)
            {
                Console.ForegroundColor = consoleColor;

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {message}");

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
