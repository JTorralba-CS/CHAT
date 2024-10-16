using Microsoft.AspNetCore.SignalR;
using System;
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

        public async Task AddToGroup(Connection X)
        {
            await Groups.AddToGroupAsync(X.ID, X.Alias);

            string message = $"Connected.";

            await Clients.Others.SendAsync("ReceiveMessage", X, message);
        }

        public async Task SendMessage(Connection X, string message)
        {
            Console.WriteLine();
            Console.WriteLine($"ChatHub.cs SendMessage {X.ID} {X.Alias} \"{message}\"");

            await Clients.All.SendAsync("ReceiveMessage", X, message);
        }
    }
}
