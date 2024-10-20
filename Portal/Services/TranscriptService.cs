using Microsoft.AspNetCore.SignalR.Client;

using Radzen;
using Standard.Models;

namespace Portal.Services
{
    public class TranscriptService
    {
        private ChatService chatService;

        public List<Message> Messages => _messages;

        private List<Message> _messages;
        private int _messagesSize;

        public TranscriptService(ChatService chatService, int messagesSize = 68)
        {
            this.chatService = chatService;

            _messagesSize = messagesSize;
            _messages = new List<Message>();

            chatService.HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
            {
                //Console.WriteLine();
                //Console.WriteLine($"Portal TranscriptService.cs ReceiveMessage {HubConnection.ConnectionId} {connection.ID} {connection.Alias} \"{message}\"");

                if (connection.ID == chatService.HubConnection.ConnectionId || connection.Alias == chatService.Connection.Alias)
                {
                    _ = Log($"{connection.Alias}: {message}", AlertStyle.Primary);
                }
                else
                {
                    _ = Log($"{connection.Alias}: {message}", AlertStyle.Secondary);
                }
            });

        }

        public async Task Send(string message)
        {
            chatService.Send(message);
        }

        private async Task Log(string message, AlertStyle alertStyle = AlertStyle.Info)
        {
            try
            {
                if (Messages.Count >= _messagesSize)
                {
                    Messages.RemoveAt(0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Portal TranscriptService.cs Log(): {e.Message}");
            }

            Messages.Add(new Message { Date = DateTime.Now, Text = message, AlertStyle = alertStyle });

            NotifyStateChanged();
        }

        public async Task Clear()
        {
            _messages.Clear();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public event Action OnChange;
    }

    public class Message
    {
        public DateTime Date { get; set; }

        public string? Text { get; set; }

        public AlertStyle AlertStyle { get; set; }
    }
}
