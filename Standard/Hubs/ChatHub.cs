using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

using Standard.Models;

namespace Standard.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async Task AddToGroup(Connection connection)
        {
            //Console.WriteLine($"Standard ChatHub.cs AddToGroup {connection.ID} {connection.Alias}");

            await Groups.AddToGroupAsync(connection.ID, connection.Alias);
            
            string message = $"[CONNECTED]";
            
            await Clients.All.SendAsync("ReceiveMessage", connection, message);
        }

        public async Task RemoveFromGroup(Connection connection)
        {
            await Groups.RemoveFromGroupAsync(connection.ID, connection.Alias);
        }

        public async Task SendMessage(Connection connection, string message)
        {
            //Console.WriteLine();
            //Console.WriteLine($"Standard ChatHub.cs SendMessage {connection.ID} {connection.Alias} {message}");

            await Clients.All.SendAsync("ReceiveMessage", connection, message);
        }

        public async Task SendRequestUsers(Connection connection)
        {
            await Clients.Group("SERVICE").SendAsync("ReceiveRequestUsers", connection);
        }

        public async Task SendResponseUsers(Connection connection, List<User> users)
        {
            if (connection.Alias == "SERVICE")
            {
                await Clients.All.SendAsync("ReceiveResponseUsers", users);
            }
            else
            {
                await Clients.Group(connection.Alias).SendAsync("ReceiveResponseUsers", users);
            }
        }
    }
}
