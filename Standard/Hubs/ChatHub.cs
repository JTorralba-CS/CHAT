using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

using Standard.Models;
using System.Text;

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
            await Groups.AddToGroupAsync(connection.ID, connection.Alias);

            string message = $"Connected.";

            await Clients.All.SendAsync("ReceiveMessage", connection, message);
        }

        public async Task RemoveFromGroup(Connection connection)
        {
            await Groups.RemoveFromGroupAsync(connection.ID, connection.Alias);
        }

        public async Task SendMessage(Connection connection, string message)
        {
            //Console.OutputEncoding = Encoding.UTF8;

            //Console.WriteLine();
            //Console.WriteLine($"ChatHub.cs SendMessage {connection.ID} {connection.Alias} {message}");

            await Clients.All.SendAsync("ReceiveMessage", connection, message);
        }
    }
}
