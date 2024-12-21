using Microsoft.AspNetCore.SignalR.Client;

using Radzen;
using Serilog;
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

        public TranscriptService(AuthenticationStateService authenticationStateService, StateService stateService, ChatService chatService, int messagesSize = 68)
        {
            StateService = stateService;

            ChatService = chatService;

            _Messages = new List<Message>(); 

            _MessagesSize = messagesSize;

            ChatService.HubConnection.On<Connection, string?>("ReceiveMessage", (connection, message) =>
            {

                if (connection.Alias == Configuration["Title"].ToUpper() & StateService.IsInitialPortal)
                {
                    StateService.UnSetIsInitialService();
                }
                else if (connection.Alias.ToUpper().Contains("PORTAL"))
                {
                    _ = authenticationStateService.GetAuthenticationStateAsync();
                }

                if (message.Contains("!!!"))
                {
                    _Notification = message = message.Replace("!!!", "").Trim();

                    if (_Notification == string.Empty)
                    {
                        _Notification = message = "We apologize for the inconvenience. System offline for maintanence. Current session(s) may expire or disconnect. Refresh browser and/or login at your later convenience.";
                    }

                    Transcribe($"{connection.Alias}: {message} [notification]", AlertStyle.Danger);

                    NotifyNewNotification();
                }
                else
                {
                    if (message == "❤")
                    {
                        message = message.Replace("❤", "❤ ");
                    }

                        if (connection.ID == ChatService.HubConnection.ConnectionId || connection.Alias == ChatService.Connection.Alias)
                    {
                        Transcribe($"{connection.Alias}: {message}", AlertStyle.Primary);
                    }
                    else
                    {
                        Transcribe($"{connection.Alias}: {message}", AlertStyle.Secondary);
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

        private Task Transcribe(string? message, AlertStyle alertStyle = AlertStyle.Info)
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
                Log.Error($"Portal TranscriptService.cs Log() Exception: {e.Message}");
            }

            Log.ForContext("Folder", "Portal").Information($"{message} [{ChatService.HubConnection.ConnectionId}]");
         
            Messages.Add(new Message { Date = DateTime.Now, Text = HTML.Format(message, 39), AlertStyle = (AlertStyle)alertStyle });

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

    public static class HTML
    {
        public static string Format(string message, int spaces)
        {
            string NextLine = "<br>" + String.Concat(Enumerable.Repeat("&nbsp", spaces));

            return message.Replace("\r\n", NextLine).Replace("\n", NextLine).Replace("\r", NextLine);
        }
    }
}
