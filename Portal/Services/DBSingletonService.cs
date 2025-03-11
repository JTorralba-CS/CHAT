using Microsoft.AspNetCore.SignalR.Client;

using Serilog;

using Standard.Databases;
using Standard.Models;

namespace Portal.Services
{
    public class DBSingletonService : Standard.Services.ChatService
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        private DBContext Database;

        public List<User> Users;

        public DBSingletonService(DBSingleton dbSingleton) : base(Configuration["ChatHub"])
        {
            string CID;

            HubConnection.On<List<User>?>("ReceiveResponseUsers", (users) =>
            {
                CID = $"{Connection.ID.Substring(0, 4).ToUpper()} $";

                try
                {
                    Database = dbSingleton.CreateDbContext(Connection.ID);

                    while (Database.UsersLocked)
                    {
                    }

                    Database.UsersLocked = true;

                    Database.Users.RemoveRange(Database.Users);

                    foreach (var record in users)
                    {
                        var userInsert = record;

                        userInsert.Name = $"{record.Name} {CID}";

                        Database.Users.Add(userInsert);
                    }

                    _ = Database.SaveChangesAsync();

                    Database.UsersLocked = false;

                    //TRACE
                    Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ ReceiveResponseUsers()");
                    foreach (var record in Database.Users.OrderBy(X => X.ID))
                    {
                        Log.ForContext("Folder", CID).Information($"{record}");
                    }

                    Users = Database.Users.OrderBy(record => record.Name).ThenBy(record => record.Password).AsQueryable().ToList();

                    NotifyStateChangedTableUsers();
                }
                catch (Exception e)
                {
                    Log.ForContext("Folder", CID).Error($"Portal DBSingletonService.cs ReceiveResponseUsers() Exception: {e.Message}");
                }
            });

            HubConnection.On<User?, char?>("ReceiveEventUpdateUser", (user, type) =>
            {
                CID = $"{Connection.ID.Substring(0, 4).ToUpper()} $";

                //TRACE
                Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ {user} {type}");

                try
                {
                    Database = dbSingleton.CreateDbContext(Connection.ID);

                    if (Database == null || !Database.Users.Any())
                    {
                        HubConnection.SendAsync("SendRequestUsersLimited");
                    }
                    else
                    {
                        while (Database.UsersLocked)
                        {
                        }

                        Database.UsersLocked = true;

                        switch (type)
                        {
                            case 'D':
                                var userDelete = Database.Users.FirstOrDefault(record => record.ID == user.ID);

                                if (userDelete != null)
                                {
                                    Database.Users.Remove(userDelete);

                                    //TRACE
                                    //Log.ForContext("Folder", CID).Information($"{userDelete} --> [DELETED]");
                                }

                                break;

                            case 'U':
                                var userUpdate = Database.Users.FirstOrDefault(record => record.ID == user.ID);

                                if (userUpdate != null)
                                {
                                    //TRACE
                                    //Log.ForContext("Folder", CID).Information($"{userUpdate}");

                                    userUpdate.Name = $"{user.Name} {CID}";
                                    userUpdate.Password = $"{userUpdate.Password} -> {user.Password}";
                                    userUpdate.Agency = user.Agency;

                                    Database.Users.Update(userUpdate);

                                    //TRACE
                                    //Log.ForContext("Folder", CID).Information($"{userUpdate}");
                                }

                                break;

                            case 'I':

                                var userInsert = user;

                                userInsert.Name = $"{user.Name} {CID}";

                                Database.Users.Add(userInsert);

                                //TRACE
                                //Log.ForContext("Folder", CID).Information($"{user} --> [INSERTED]");

                                break;
                        }

                        Database.SaveChangesAsync();

                        Database.UsersLocked = false;

                        Users = Database.Users.OrderBy(record => record.Name).ThenBy(record => record.Password).AsQueryable().ToList();

                        NotifyStateChangedTableUsers();
                    }
                }
                catch (Exception e)
                {
                    Log.ForContext("Folder", CID).Error($"Portal DBSingletonService.cs ReceiveEventUpdateUser() Exception: {e.Message}");
                }
            });

            HubConnection.On<DateTime>("ReceiveServiceActive", (dateTime) =>
            {
                CID = $"{Connection.ID.Substring(0, 4).ToUpper()} $";

                Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ ReceiveServiceActive()");

                _ = SendRequestData();

                // Update LoginPage.razor Drop-Down List
                NotifyStateChangedServiceActive();
            });

            _ = SendRequestData();
        }

        public async Task SendRequestData()
        {
            await HubConnection.SendAsync("SendRequestUsersLimited");
        }

        private void NotifyStateChangedTableUsers()
        {
            OnChangeTableUsers.Invoke();
        }

        public event Action OnChangeTableUsers;

        private void NotifyStateChangedServiceActive()
        {
            OnChangeServiceActive.Invoke();
        }

        public event Action OnChangeServiceActive;
    }
}
