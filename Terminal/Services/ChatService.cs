
//OK

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

using Standard.Functions;
using Standard.Models;

namespace Terminal.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public ChatService() : base(Configuration["ChatHub"])
        {
            string Title = Configuration["Title"];

            Console.Title = $"{Title} (Terminal)";

            Connection.Alias = "Terminal";

            HubConnection.On<Connection, string?>("ReceiveMessage", (connection, message) =>
            {
                if (connection.ID == HubConnection.ConnectionId || connection.Alias == Connection.Alias)
                {
                    Core.WriteConsole($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                }
                else
                {
                    Core.WriteConsole($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                }
            });
        }
    }
}
