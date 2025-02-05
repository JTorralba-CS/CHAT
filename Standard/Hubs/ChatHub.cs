//OK

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Serilog;

using Standard.Functions;
using Standard.Models;


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

            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("OnConnectedAsync()", Context.ConnectionId, $"{connection.ID} ({connection.Alias})", "[connected]"));

            await Groups.AddToGroupAsync(connection.ID, connection.Alias);

            await Clients.Client(connection.ID).SendAsync("ReceiveConnected", connection);

            await base.OnConnectedAsync();
        }

        private bool IsAlreadyConnected(Connection connection)
        {
            if (AlreadyConnected.TryGetValue(connection.ID, out string alias))
            {
                if (connection.Alias.ToUpper() == alias.ToUpper())
                {
                    Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("IsAlreadyConnected()", Context.ConnectionId, $"{connection.ID} ({connection.Alias})", "[true]"));

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
            if (connection.Alias.ToUpper() == Title.ToUpper() || connection.Alias.ToUpper() == "PORTAL")
            {
                await SendServiceActive();
            }

            if (!IsAlreadyConnected(connection) && !connection.Alias.ToUpper().Contains("PORTAL"))
            {
                if (!AlreadyConnected.TryGetValue(connection.ID, out _))
                {
                    AlreadyConnected.Add(connection.ID, connection.Alias.ToUpper());

                    Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("AddToGroup()", Context.ConnectionId, $"{connection.ID} ({connection.Alias})", "[connected]"));

                    await Clients.All.SendAsync("ReceiveMessage", connection, $"[connected]");
                }

                await Groups.AddToGroupAsync(connection.ID, connection.Alias);
            }
        }

        public async Task RemoveFromGroup(Connection connection)
        {
            await Groups.RemoveFromGroupAsync(connection.ID, connection.Alias);

            if (AlreadyConnected.TryGetValue(connection.ID, out _))
            {
                AlreadyConnected.Remove(connection.ID);

                Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("RemoveFromGroup()", Context.ConnectionId, $"{connection.ID} ({connection.Alias})", "[disconnected]"));
            }
        }

        public async Task SendMessage(Connection connection, string message)
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("SendMessage()", Context.ConnectionId, $"{connection.ID} ({connection.Alias})", $"{message}"));

            if (message != "_")
            {
                await Clients.All.SendAsync("ReceiveMessage", connection, message);
            }
            else
            {
                await Clients.Group(Title).SendAsync("ReceiveMessage", connection, message);
            }
        }

        public async Task SendMessageToSender(Connection connection, string message)
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("SendMessageToSender()", Context.ConnectionId, $"{connection.ID} ({connection.Alias})", $"{message}"));

            await Clients.Group(connection.Alias).SendAsync("ReceiveMessage", connection, message);
        }

        public async Task SendRequestUsers()
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("SendRequestUsers()", Context.ConnectionId));

            await Clients.Group(Title).SendAsync("ReceiveRequestUsers");
        }

        public async Task SendResponseUsers(List<User> users)
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("SendResponseUsers()", Context.ConnectionId));

            await Clients.All.SendAsync("ReceiveResponseUsers", users);
        }

        public async Task SendRequestLogin(Connection connection, User user)
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("SendRequestLogin()", Context.ConnectionId, $"{connection.ID} ({connection.Alias})", $"{user.ID} {user.Name} ********"));

            await Clients.Group(Title).SendAsync("ReceiveRequestLogin", connection, user);
        }

        public async Task SendResponseLogin(Connection connection, User user, bool authenticated)
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("SendResponseLogin()", Context.ConnectionId, $"{connection.ID} ({connection.Alias})", $"{user.ID} {user.Name} ({authenticated})"));

            await Clients.Group(connection.ID).SendAsync("ReceiveResponseLogin", user, authenticated);
        }

        public async Task SendRequestLogout(Connection connection, User user)
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("SendRequestLogout()", Context.ConnectionId, $"{connection.ID} {connection.Alias}", $"{user.ID} {user.Name}"));

            await Clients.Group(Title).SendAsync("ReceiveRequestLogout", connection, user);
        }

        public async Task SendResponseLogout(Connection connection)
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("SendResponseLogout()", Context.ConnectionId, $"{connection.ID} {connection.Alias}"));

            await Clients.Group(connection.ID).SendAsync("ReceiveResponseLogout");
        }

        public async Task SendServiceActive()
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format("SendServiceActive()", Context.ConnectionId));

            await Clients.All.SendAsync("ReceiveServiceActive", DateTime.Now);
        }

        public async Task SendEventUpdateUser(Connection connection, User user, char type)
        {
            Log.ForContext("Folder", "ChatHub").Information(SeriLog.Format($"SendEventUpdateUser()", Context.ConnectionId, connection.ID, $"{user} {type}"));

            await Clients.Group(connection.ID).SendAsync("ReceiveEventUpdateUser", user, type);
        }
    }
}
