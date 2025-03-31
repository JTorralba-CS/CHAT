using Microsoft.AspNetCore.SignalR.Client;

using Serilog;

using Standard.Models;

namespace Portal.Services
{
    public class LoginService
    {
        private readonly StateService StateService;

        public readonly ChatService ChatService;

        public User? User => _User;

        private User? _User = null;

        public bool Authenticated => _Authenticated;

        private bool _Authenticated = false;

        public LoginService(StateService stateService, ChatService chatService, DBSingletonService dbSingletonService)
        {
            dbSingletonService.OnChangeServiceActive += async () =>
            {               
                if (User != null)
                {
                    Authenticate(User);
                }
                else
                {
                    DeAuthenticate();
                }
            };

            StateService = stateService;

            ChatService = chatService;

            _Authenticated = false;

            _User = null;

            ChatService.HubConnection.On<User, bool>("ReceiveResponseLogin", (user, authenticated) =>
            {
                if (authenticated)
                {
                    //TRACE
                    Log.ForContext("Folder", "Portal").Information($"Portal LoginService.cs ReceiveResponseLogin(): connection = {ChatService.Connection}, LoginService.User = {User}, user = {user} [authenticated]");

                    _Authenticated = true;

                    _User = user;

                    _ = ChatService.SetAlias(user.Name);
                }
                else if (User == null || User.ID == user.ID)
                {
                    _ = DeAuthenticate();
                }

                NotifyStateChangedAuthenticated();
            });

            ChatService.HubConnection.On("ReceiveResponseLogout", async () =>
            {
                await Logout();
            });
        }

        public async Task Logout()
        {
            _Authenticated = false;

            _User = null;

            _ = ChatService.RemoveAlias();

            NotifyStateChangedDeAuthenticated();
        }

        public async Task RequestUsers()
        {
            try
            {
                while (!ChatService._HubConnected && StateService.IsInitialService)
                {
                }

                await ChatService.HubConnection.SendAsync("SendRequestUsersLimited");
            }
            catch (Exception e)
            {
                Log.ForContext("Folder", "Portal").Error($"Portal LoginService.cs RequestUsers() Exception: {e.Message}");
            }
        }

        public async Task Authenticate(User? user)
        {
            try
            {
                while (!ChatService._HubConnected && StateService.IsInitialService)
                {
                }

                //TRACE
                Log.ForContext("Folder", "Portal").Information($"Portal LoginService.cs SendRequestLogin(): connection = {ChatService.Connection}, LoginService.User = {User}, user = {user}");

                await ChatService.HubConnection.SendAsync("SendRequestLogin", ChatService.Connection, user);
            }
            catch (Exception e)
            {
                Log.ForContext("Folder", "Portal").Error($"Portal LoginService.cs Authenticate() Exception: {e.Message}");
            }
        }

        public async Task DeAuthenticate()
        {

            //TRACE
            Log.ForContext("Folder", "Portal").Information($"Portal LoginService.cs DeAuthenticate(): connection = {ChatService.Connection}, LoginService.User = {User}");

            try
            {
                while (!ChatService._HubConnected && StateService.IsInitialService)
                {
                }

                if (User != null)
                {
                    await ChatService.HubConnection.SendAsync("SendRequestLogout", ChatService.Connection, User);
                }
                else
                {
                    await Logout();
                }
            }
            catch (Exception e)
            {
                Log.ForContext("Folder", "Portal").Error($"Portal LoginService.cs DeAuthenticate() Exception: {e.Message}");
            }
        }

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
