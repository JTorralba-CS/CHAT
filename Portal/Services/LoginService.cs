using Microsoft.AspNetCore.SignalR.Client;

using Standard.Models;

namespace Portal.Services
{
    public class LoginService
    {
        private ChatService chatService;

        public List<User> Users => _users;

        private List<User> _users;

        public LoginService(ChatService chatService)
        {
            this.chatService = chatService;

            _users = new List<User>();

            chatService.HubConnection.On<List<User>>("ReceiveResponseUsers", (users) =>
            {
                //Console.WriteLine();
                //Console.WriteLine($"Portal LoginService.cs ReceiveResponseUsers {HubConnection.ConnectionId}");

                _ = SetUsers(users);
            });
        }

        public async Task GetUsers()
        {
            await chatService.HubConnection.SendAsync("SendRequestUsers", chatService.Connection);
        }

        private async Task SetUsers(List<User> users)
        {
            _users = users;

            NotifyStateChanged();
        }

        public async Task Authenticate(User user)
        {
            //Console.WriteLine($"{user.ID} {user.Name}");
        }

        private void NotifyStateChanged()
        {
            _users = _users.OrderBy(user => user.Name.ToUpper().Trim().Replace("  ", " ").Replace("  ", " ")).ToList();

            OnChange?.Invoke();
        }

        public event Action OnChange;
    }
}
