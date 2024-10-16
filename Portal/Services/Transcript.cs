using Radzen;

namespace Portal.Services
{
    public class Transcript
    {
        public List<Message> Messages => _messages;

        private List<Message> _messages;
        private int _messagesSize;

        public Transcript(int messagesSize = 68)
        {
            _messagesSize = messagesSize;
            _messages = new List<Message>();
        }

        public async Task Log(string message, AlertStyle alertStyle = AlertStyle.Info)
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
                Console.WriteLine($"Transcript.cs Log(): {e.Message}");
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
