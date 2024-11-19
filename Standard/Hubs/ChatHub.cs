//OK

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Standard.Models;
using Standard.Functions;

namespace Standard.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("Standard.json").Build();

        private static readonly string Title = Configuration["Title"].ToUpper();

        private static readonly Dictionary<string, string> AlreadyConnected = new Dictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            Connection connection = new Connection
            {
                ID = Context.ConnectionId,
                Alias = Context.ConnectionId
            };

            await Groups.AddToGroupAsync(connection.ID, connection.Alias);

            await base.OnConnectedAsync();
        }

        private bool IsAlreadyConnected(Connection connection)
        {
            if (AlreadyConnected.TryGetValue(connection.ID, out string alias))
            {
                if (connection.Alias == alias)
                {
                    return true;
                }
            }
            else
            {
            };

            return false;
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            await base.OnDisconnectedAsync(e);
        }

        public async Task AddToGroup(Connection connection)
        {
            if (connection.Alias == Title || connection.Alias == "PORTAL")
            {
                await SendServiceActive();
            }

            if (!IsAlreadyConnected(connection) && !connection.Alias.Contains("PORTAL"))
            {
                if (!AlreadyConnected.TryGetValue(connection.ID, out _))
                {
                    AlreadyConnected.Add(connection.ID, connection.Alias);

                    await Clients.All.SendAsync("ReceiveMessage", connection, $"[connected]");
                }

                await Groups.AddToGroupAsync(connection.ID, connection.Alias);
            }

            Core.WriteInfo();
        }

        public async Task RemoveFromGroup(Connection connection)
        {
            await Groups.RemoveFromGroupAsync(connection.ID, connection.Alias);
            if (AlreadyConnected.TryGetValue(connection.ID, out _))
            {
                AlreadyConnected.Remove(connection.ID);
            }
        }

        public async Task SendMessage(Connection connection, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", connection, message);
        }

        public async Task SendMessageToSender(Connection connection, string message)
        {
            await Clients.Group(connection.Alias).SendAsync("ReceiveMessage", connection, message);
        }

        public async Task SendRequestUsers()
        {
            await Clients.Group(Title).SendAsync("ReceiveRequestUsers");
        }

        public async Task SendResponseUsers(List<User> users)
        {
            await Clients.All.SendAsync("ReceiveResponseUsers", users);
        }

        public async Task SendRequestLogin(Connection connection, User user)
        {
            await Clients.Group(Title).SendAsync("ReceiveRequestLogin", connection, user);
        }

        public async Task SendResponseLogin(Connection connection, User user, bool authenticated)
        {
            await Clients.Group(connection.ID).SendAsync("ReceiveResponseLogin", user, authenticated);
        }

        public async Task SendRequestLogout(Connection connection, User user)
        {
            await Clients.Group(Title).SendAsync("ReceiveRequestLogout", connection, user);
        }

        public async Task SendResponseLogout(Connection connection)
        {
            await Clients.Group(connection.ID).SendAsync("ReceiveResponseLogout");
        }

        public async Task SendServiceActive()
        {
            await Clients.All.SendAsync("ReceiveServiceActive", DateTime.Now);
        }
    }
}
