using Microsoft.AspNetCore.SignalR.Client;
using Standard.Models;
using System;
using System.Threading.Tasks;

namespace Standard.Services
{
    public class ChatService
    {
        public HubConnection hubConnection;

        public Connection Connection;

        private bool hubConnected => hubConnection?.State == HubConnectionState.Connected;

        public ChatService()
        {
            Connection = new Connection
            {
                ID = string.Empty,
                Alias = string.Empty
            };

            _ = HubConnect();
        }
        
        private async Task HubConnect()
        {
            try
            {
                hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:9110/chathub")
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(16), TimeSpan.FromSeconds(32), TimeSpan.FromSeconds(64), TimeSpan.FromSeconds(128) })
                .Build();

                hubConnection.Closed += HubDisconnected;
                hubConnection.Reconnecting += HubDisconnected;
                hubConnection.Reconnected += HubConnected;

                await hubConnection.StartAsync();

                while (!hubConnected)
                {
                }

                await HubConnected(string.Empty);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ChatService.cs HubConnect(): {e.Message}");

                await hubConnection.StartAsync();
            }
        }

        private async Task HubDisconnected(Exception e)
        {
        }

        private async Task HubConnected(string s)
        {
            Connection.ID = hubConnection.ConnectionId;

            if (Connection.Alias is null || Connection.Alias == string.Empty)
            {
                Connection.Alias = hubConnection.ConnectionId;
            }

            await SetConnection(Connection.Alias);
        }

        public async Task Send(string message)
        {
            if (message != null && message != string.Empty)
            {
                try
                {
                    if (hubConnection != null && hubConnected)
                    {
                        await hubConnection.SendAsync("SendMessage", Connection, message);
                    }
                    else
                    {
                        await hubConnection.StartAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ChatService.cs Send(): {e.Message}");

                    await hubConnection.StartAsync();
                }
            }
        }

        public async Task SetConnection(string alias)
        {
            Connection.Alias = alias;
            Connection.ID = hubConnection.ConnectionId;
            await hubConnection.SendAsync("AddToGroup", Connection);
        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection != null)
            {
                await hubConnection.DisposeAsync();
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