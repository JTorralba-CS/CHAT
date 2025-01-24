//OK

using Microsoft.AspNetCore.SignalR.Client;

using Serilog;

using Standard.Databases;
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
                using (var tables = new IMDB())
                {
                    try
                    {
                        tables.Users.RemoveRange(tables.Users);

                        tables.Users.AddRange(users);

                        tables.SaveChangesAsync();

                        _Users = tables.Users.AsQueryable().ToList();
                    }
                    catch (Exception e)
                    {
                        Log.ForContext("Folder", "Portal").Error($"Portal LoginService.cs ReceiveResponseUsers() Exception: {e.Message}");
                    }
                }

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

            ChatService.HubConnection.On<User?, char?>("ReceiveEventUpdateUser", (user, type) =>
            {
                //TRACE
                //Log.ForContext("Folder", "Portal").Error($"Portal LoginService.cs ReceiveEventUpdateUser(): {user.ID} {user.Name} {user.Password} {type} {ChatService.Connection.ID} {ChatService.Connection.Alias}");

                using (var tables = new IMDB())
                {
                    try
                    {
                        switch (type)
                        {
                            case 'D':
                                tables.Users.Remove(user);
                                break;
                            case 'U':
                                tables.Users.Update(user);
                                break;
                            case 'I':
                                tables.Users.Add(user);
                                break;
                        }

                        tables.SaveChanges();

                        _Users = tables.Users.AsQueryable().ToList();
                    }
                    catch (Exception e)
                    {
                        Log.ForContext("Folder", "Portal").Error($"Portal LoginService.cs ReceiveEventUpdateUser() Exception: {e.Message}");
                    }
                }

                NotifyStateChangedUsers();


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

                await ChatService.HubConnection.SendAsync("SendRequestLogin", ChatService.Connection, user);
            }
            catch (Exception e)
            {
                Log.ForContext("Folder", "Portal").Error($"Portal LoginService.cs Authenticate() Exception: {e.Message}");
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
                Log.ForContext("Folder", "Portal").Error($"Portal LoginService.cs DeAuthenticate() Exception: {e.Message}");
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
