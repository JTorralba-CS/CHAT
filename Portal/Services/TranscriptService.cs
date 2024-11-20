using Microsoft.AspNetCore.SignalR.Client;

using Radzen;

using Standard.Functions;
using Standard.Models;

namespace Portal.Services
{
    public class TranscriptService
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        private readonly StateService StateService;

        private readonly ChatService ChatService;

        public List<Message> Messages => _Messages;

        private readonly List<Message> _Messages;

        private readonly int _MessagesSize;

        public string Notification => _Notification;

        private string _Notification;

        public TranscriptService(StateService stateService, ChatService chatService, int messagesSize = 68)
        {
            StateService = stateService;

            ChatService = chatService;

            _Messages = new List<Message>(); 

            _MessagesSize = messagesSize;

            ChatService.HubConnection.On<Connection, string?>("ReceiveMessage", (connection, message) =>
            {
                if (connection.Alias == Configuration["Title"].ToUpper())
                {
                    StateService.UnSetIsInitialService();
                }

                if (message.Contains("!!!"))
                {
                    _Notification = message.Replace("!!!", "").Trim();

                    NotifyNewNotification();
                }
                else
                {
                    if (connection.ID == ChatService.HubConnection.ConnectionId || connection.Alias == ChatService.Connection.Alias)
                    {
                        Log($"{connection.Alias}: {message}", AlertStyle.Primary);
                    }
                    else
                    {
                        Log($"{connection.Alias}: {message}", AlertStyle.Secondary);
                    }
                }
            });
        }

        public async Task Send(string? message)
        {
            while (!ChatService._HubConnected && StateService.IsInitialService)
            {
            }

            await ChatService.Send(message);
        }

        private Task Log(string? message, AlertStyle alertStyle = AlertStyle.Info)
        {
            try
            {
                if (Messages.Count >= _MessagesSize)
                {
                    Messages.RemoveAt(0);
                }
            }
            catch (Exception e)
            {
                Core.WriteError($"Portal TranscriptService.cs Log() Exception: {e.Message}");
            }

            Messages.Add(new Message { Date = DateTime.Now, Text = message, AlertStyle = (AlertStyle)alertStyle });

            NotifyNewMessage();

            return Task.CompletedTask;
        }

        public Task ClearMessages()
        {
            _Messages.Clear();

            return Task.CompletedTask;
        }

        public Task ClearNotification()
        {
            _Notification = string.Empty;

            return Task.CompletedTask;
        }

        private void NotifyNewMessage() => OnNewMessage?.Invoke();

        public event Action OnNewMessage;

        private void NotifyNewNotification() => OnNewNotification?.Invoke();

        public event Action OnNewNotification;
    }

    public class Message
    {
        public DateTime Date { get; set; }

        public string? Text { get; set; }

        public AlertStyle AlertStyle { get; set; }
    }
}
