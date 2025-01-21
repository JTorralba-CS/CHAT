//OK

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;

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
                using (var context = new IMDB())
                {
                    try
                    {
                        context.Users.AddRange(users);
                        context.SaveChanges();
                    }
                    catch
                    {
                    }
                }

               // Drop-Down User List

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

            var timer = new System.Timers.Timer(5000);
            timer.Elapsed += (s, e) =>
            {
                using (var context = new IMDB())
                {
                    Random random = new Random();

                    int ID = random.Next(1, 100);

                    foreach (var userChange in context.Users)
                    {
                        userChange.Password = Guid.NewGuid().ToString();

                        if (userChange.ID == ID)
                        {
                            userChange.Name = $"{userChange.Name} XXX";
                        }
                    }

                    var userAdd = new User()
                    {
                        ID = context.Users.Count() + 1,
                        Name = $"{Guid.NewGuid().ToString()} XXX",
                        Password = Guid.NewGuid().ToString()
                    };

                    context.Users.Add(userAdd);

                    context.SaveChanges();

                }

                NotifyStateChangedUsers();
            };

            timer.Start();
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

    public class IMDB : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("IMDB");
        }
    }
}
