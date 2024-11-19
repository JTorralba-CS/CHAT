//OK

using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text;
using System.Threading.Tasks;

using Standard.Functions;
using Standard.Models;

namespace Standard.Services
{
    public class ChatService
    {
        public HubConnection HubConnection;

        public Connection Connection;

        public bool _HubConnected => HubConnection?.State == HubConnectionState.Connected;

        private readonly string URL;

        public ChatService(string url)
        {
            try
            {
                //Windows Services do not like writing to interactive console. Hence, TOPSHELF will fail to install service.
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch (Exception e)
            {
            }

            URL = url;

            Connection = new Connection();

            _ = HubConnect();
        }

        private async Task HubConnect()
        {
            try
            {
                HubConnection = new HubConnectionBuilder()
                .WithUrl(URL)
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8), TimeSpan.FromSeconds(16), TimeSpan.FromSeconds(32), TimeSpan.FromSeconds(64), TimeSpan.FromSeconds(128) })
                .Build();

                HubConnection.Closed += HubDisconnected;
                HubConnection.Reconnecting += HubDisconnected;
                HubConnection.Reconnected += HubConnected;

                await HubConnection.StartAsync();

                while (!_HubConnected)
                {
                }

                await HubConnected(string.Empty);
            }
            catch (Exception e)
            {
                Core.WriteError($"Standard ChatService.cs HubConnect() Exception: {e.Message}");

                await HubConnection.StartAsync();
            }
        }

        private async Task HubDisconnected(Exception e)
        {
            await HubConnection.StartAsync();
        }

        private async Task HubConnected(string args)
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
                        if (HubConnection != null && _HubConnected)
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
                        Core.WriteError($"Standard ChatService.cs Send() Exception: {e.Message}");

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

        public async Task RemoveAlias()
        {
            await HubConnection.SendAsync("RemoveFromGroup", Connection);
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
