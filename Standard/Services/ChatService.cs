using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

using Standard.Models;
using Standard.Functions;

namespace Standard.Services
{
    public class ChatService
    {
        public HubConnection HubConnection;

        public Connection Connection;

        private bool hubConnected => HubConnection?.State == HubConnectionState.Connected;

        private static string URL;

        //public ChatService()
        //{
        //    Connection = new Connection();

        //    _ = HubConnect();
        //}

        public ChatService(string url)
        {
            URL = url;

            Connection = new Connection();

            _ = HubConnect();
        }

        private async Task HubConnect()
        {
            try
            {
                HubConnection = new HubConnectionBuilder()
                //.WithUrl("https://localhost:9110/chathub")
                .WithUrl(URL)
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(16), TimeSpan.FromSeconds(32), TimeSpan.FromSeconds(64), TimeSpan.FromSeconds(128) })
                .Build();

                HubConnection.Closed += HubDisconnected;
                HubConnection.Reconnecting += HubDisconnected;
                HubConnection.Reconnected += HubConnected;

                await HubConnection.StartAsync();

                while (!hubConnected)
                {
                }

                await HubConnected(string.Empty);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ChatService.cs HubConnect(): {e.Message}");

                await HubConnection.StartAsync();
            }
        }

        private async Task HubDisconnected(Exception e)
        {
            await HubConnection.StartAsync();
        }

        private async Task HubConnected(string s)
        {
            Connection.ID = HubConnection.ConnectionId;

            if (Connection.Alias is null || Connection.Alias == string.Empty)
            {
                Connection.Alias = HubConnection.ConnectionId;
            }

            await SetAlias(Connection.Alias);
        }

        public async Task Send(string message)
        {
            var Argument = Core.SplitSpaceInput(message);

            if (Argument[0] == ".")
            {
                await SetAlias(Argument[1]);
            }
            else
            {
                if (message != null && message != string.Empty)
                {

                    try
                    {
                        if (HubConnection != null && hubConnected)
                        {
                            await HubConnection.SendAsync("SendMessage", Connection, message);
                        }
                        else
                        {
                            await HubConnection.StartAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"ChatService.cs Send(): {e.Message}");

                        await HubConnection.StartAsync();
                    }
                }

            }
        }

        public async Task SetAlias(string alias)
        {
            Connection.Alias = alias.ToUpper();

            await HubConnection.SendAsync("AddToGroup", Connection);
        }

        public async ValueTask DisposeAsync()
        {
            if (HubConnection != null)
            {
                await HubConnection.DisposeAsync();
            }
        }
    }
}

// Red Heart:
// "❤"
// "\u2764"

// Blue Heart:
// "\U0001F499"

// Green Heart:
// "\U0001F49A"

// Yellow Heart:
// "\U0001F49B"

// Purple Heart:
// "\U0001F49C"

// Smiley:
// "\U0001F60A"
