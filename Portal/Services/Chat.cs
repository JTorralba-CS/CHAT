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

            hubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
            {
                Console.WriteLine();
                Console.WriteLine($"Chat.cs ReceiveMessage {hubConnection.ConnectionId} {connection.ID} {connection.Alias} \"{message}\"");

                if (connection.ID == hubConnection.ConnectionId || connection.Alias == Connection.Alias)
                {
                    _ = transcript.Log($"{connection.Alias}: {message}", AlertStyle.Primary);
                }
                else
                {
                    _ = transcript.Log($"{connection.Alias}: {message}", AlertStyle.Secondary);
                }
            });
        }
    }
}
