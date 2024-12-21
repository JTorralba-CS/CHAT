//OK

using Microsoft.AspNetCore.SignalR.Client;

using Serilog;

using Standard.Models;

namespace Portal.Services
{
    public class LoginService
    {
        private readonly StateService StateService;

        private readonly ChatService ChatService;

        private readonly TranscriptService TranscriptService;

        public List<User>? Users => _Users;

        private List<User>? _Users = null;

        public User? User => _User;

        private User? _User = null;

        public bool Authenticated => _Authenticated;

        private bool _Authenticated = false;

        public LoginService(StateService stateService, ChatService chatService, TranscriptService transcriptService)
        {
            StateService = stateService;

            ChatService = chatService;

            TranscriptService = transcriptService;

            _Users = new List<User>();

            _User = null;

            _Authenticated = false;

            ChatService.HubConnection.On<List<User>?>("ReceiveResponseUsers", (users) =>
            {
                _Users = users;

                NotifyStateChangedUsers();
            });

            ChatService.HubConnection.On<User, bool>("ReceiveResponseLogin", (user, authenticated) =>
            {
                if (authenticated)
                {
                    _Authenticated = true;

                    _User = user;

                    _ = ChatService.SetAlias(user.Name);
                }
                else
                {
                    _ = DeAuthenticate();
                }

                NotifyStateChangedAuthenticated();
            });

            ChatService.HubConnection.On("ReceiveResponseLogout", () =>
            {
                _Authenticated = false; 
                
                _User = null;

                _ = TranscriptService.ClearMessages();

                _ = ChatService.RemoveAlias();

                NotifyStateChangedDeAuthenticated();

            });

            ChatService.HubConnection.On<DateTime>("ReceiveServiceActive", (dateTime) =>
            {
                ChatService.HubConnection.SendAsync("SendRequestLogin", ChatService.Connection, User);
            });
        }

        public async Task RequestUsers()
        {
            try
            {
                while (!ChatService._HubConnected && StateService.IsInitialService)
                {
                }

                await ChatService.HubConnection.SendAsync("SendRequestUsers");
            }
            catch (Exception e)
            {
                Log.Error($"Portal LoginService.cs RequestUsers() Exception: {e.Message}");
            }
        }

        public async Task Authenticate(User? user)
        {
            try
            {
                while (!ChatService._HubConnected && StateService.IsInitialService)
                {
                }

                await ChatService.HubConnection.SendAsync("SendRequestLogin", ChatService.Connection, user);
            }
            catch (Exception e)
            {
                Log.Error($"Portal LoginService.cs Authenticate() Exception: {e.Message}");
            }
        }

        public async Task DeAuthenticate()
        {
            try
            {
                while (!ChatService._HubConnected && StateService.IsInitialService)
                {
                }

                await ChatService.HubConnection.SendAsync("SendRequestLogout", ChatService.Connection, User);
            }
            catch (Exception e)
            {
                Log.Error($"Portal LoginService.cs DeAuthenticate() Exception: {e.Message}");
            }
        }

        private void NotifyStateChangedUsers()
        {
            OnChangeUsers.Invoke();
        }

        public event Action OnChangeUsers;

        private void NotifyStateChangedAuthenticated()
        {
            OnChangeAuthenticated.Invoke();
        }

        public event Action OnChangeAuthenticated;

        private void NotifyStateChangedDeAuthenticated()
        {
            OnChangeDeAuthenticated.Invoke();
        }

        public event Action OnChangeDeAuthenticated;
    }
}
