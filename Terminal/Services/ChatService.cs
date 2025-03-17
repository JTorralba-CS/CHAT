
//OK

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

using Serilog;

using Standard.Functions;
using Standard.Models;
using System.Text.RegularExpressions;

namespace Terminal.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public ChatService() : base(Configuration["ChatHub"])
        {
            string Title = Configuration["Title"];

            Console.Title = $"{Title} (Terminal)";

            Connection.Alias = "TERMINAL";

            HubConnection.On<Connection, string?>("ReceiveMessage", (connection, message) =>
            {

                if (message.ToUpper().Contains("REDLIGHT"))
                {
                    message = Regex.Replace(message, "REDLIGHT", "", RegexOptions.IgnoreCase).Trim();

                    if (message == string.Empty)
                    {
                        message = "We apologize for the inconvenience. System offline for maintanence. Current session(s) may expire or disconnect. Refresh browser and/or login at your later convenience.";
                    }

                    Core.WriteConsole($"{connection.Alias}: {message} [notification]", ConsoleColor.Red);

                    Log.Information($"{connection.Alias}: {message} [notification]");
                    //Log.ForContext("Folder", "Terminal").Information($"{connection.Alias}: {message} [notification]");
                }
                else
                {
                    if (connection.ID == HubConnection.ConnectionId || connection.Alias.ToUpper() == Connection.Alias.ToUpper())
                    {
                        Core.WriteConsole($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        Core.WriteConsole($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                    }

                    Log.Information($"{connection.Alias}: {message}");
                    //Log.ForContext("Folder", "Terminal").Information($"{connection.Alias}: {message}");
                }

            });

            HubConnection.On<DateTime>("ReceiveServiceActive", (dateTime) =>
            {
                DateTime RecieveServiceActiveDateTime = DateTime.Now;

                Core.WriteInfo($"ReceiveServiceActive() [{RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}]");

                Log.Information($"ReceiveServiceActive() [{RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}]");
                //Log.ForContext("Folder", "Terminal").Information($"ReceiveServiceActive() [{RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}]");
            });

            HubConnection.On<Connection>("ReceiveConnected", (connection) =>
            {
                Core.WriteInfo($"ReceiveConnected {connection.ID} <{Connection.Alias}>");

                Log.Information($"ReceiveConnected {connection.ID} <{Connection.Alias}>");

                Connection.ID = connection.ID;

                _ = SetAlias(Connection.Alias);
            });
        }
    }
}
