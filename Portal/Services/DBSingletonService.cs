using Microsoft.AspNetCore.SignalR.Client;

using Serilog;

using Portal.Databases;
using Standard.Models;

namespace Portal.Services
{
    public class DBSingletonService : Standard.Services.ChatService
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public DBContext Database;

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
                        record.Name = $"{record.Name} {CID}";

                        Database.Users.Add(record);
                    }

                    _ = Database.SaveChangesAsync();

                    Database.UsersLocked = false;

                    //TRACE
                    Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ ReceiveResponseUsers()");
                    foreach (var record in Database.Users.OrderBy(X => X.ID))
                    {
                        Log.ForContext("Folder", CID).Information($"{record}");
                    }
                }
                catch (Exception e)
                {
                    Log.ForContext("Folder", CID).Error($"Portal DBSingletonService.cs ReceiveResponseUsers() Exception: {e.Message}");
                }

                NotifyStateChangedTableUsers();
            });

            HubConnection.On<User?, char?>("ReceiveEventUpdateUser", (user, type) =>
            {
                CID = $"{Connection.ID.Substring(0, 4).ToUpper()} $";

                //TRACE
                Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ {user} {type}");

                try
                {
                    Database = dbSingleton.CreateDbContext(Connection.ID);

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
                }
                catch (Exception e)
                {
                    Log.ForContext("Folder", CID).Error($"Portal DBSingletonService.cs ReceiveEventUpdateUser() Exception: {e.Message}");
                }

                NotifyStateChangedTableUsers();
            });

            HubConnection.On<DateTime>("ReceiveServiceActive", (dateTime) =>
            {
                CID = $"{Connection.ID.Substring(0, 4).ToUpper()} $";

                Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ ReceiveServiceActive()");

                HubConnection.SendAsync("SendRequestUsersLimited");
            });

            HubConnection.SendAsync("SendRequestUsersLimited");
        }

        private void NotifyStateChangedTableUsers()
        {
            OnChangeTableUsers.Invoke();
        }

        public event Action OnChangeTableUsers;
    }
}
