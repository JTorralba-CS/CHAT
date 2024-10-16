using Microsoft.AspNetCore.SignalR.Client;

using Radzen;
using Standard.Services;
using Standard.Models;

namespace Portal.Services
{
    public class Chat : ChatService
    {
        private Transcript transcript;

        public Chat(Transcript transcript)
        {
            this.transcript = transcript;

            HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
            {
                //Console.WriteLine();
                //Console.WriteLine($"Chat.cs ReceiveMessage {HubConnection.ConnectionId} {connection.ID} {connection.Alias} \"{message}\"");

                if (connection.ID == HubConnection.ConnectionId || connection.Alias == Connection.Alias)
                {
                    _ = transcript.Log($"{connection.Alias}: {message}", AlertStyle.Primary);
                }
                else
                {
                    _ = transcript.Log($"{connection.Alias}: {message}", AlertStyle.Secondary);
                }
            });

            _ = Send(". Portal");
        }
    }
}
