using Microsoft.AspNetCore.SignalR.Client;
using Serilog;

using Standard.Databases;
using Standard.Models;

namespace Portal.Services
{
    public class DBScopedService
    {
        public ChatService ChatService;

        private LoginService LoginService;

        private DBContext Database;

        public List<User> Users;

        public List<Unit> Units;

        public DBScopedService(ChatService chatService, DBScoped dbScoped, LoginService loginService)
        {
            ChatService = chatService;

            LoginService = loginService;

            string CID;

            ChatService.HubConnection.On<List<User>?>("ReceiveResponseUsers", (users) =>
            {
                CID = ChatService.Connection.ID.Substring(0, 4).ToUpper();

                try
                {
                    Database = dbScoped.CreateDbContext(ChatService.Connection.ID);

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
                    Log.ForContext("Folder", CID).Error($"Portal DBScopedService.cs ReceiveResponseUsers() Exception: {e.Message}");
                }
            });

            ChatService.HubConnection.On<User?, char?>("ReceiveEventUpdateUser", (user, type) =>
            {
                CID = ChatService.Connection.ID.Substring(0, 4).ToUpper();

                //TRACE
                Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ {user} {type}");

                try
                {
                    Database = dbScoped.CreateDbContext(ChatService.Connection.ID);

                    if (Database == null || !Database.Users.Any())
                    {
                        ChatService.HubConnection.SendAsync("SendRequestUsersLimited");
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
                    Log.ForContext("Folder", CID).Error($"Portal DBScopedService.cs ReceiveEventUpdateUser() Exception: {e.Message}");
                }
            });

            ChatService.HubConnection.On<List<Unit>?>("ReceiveResponseUnits", (units) =>
            {
                CID = ChatService.Connection.ID.Substring(0, 4).ToUpper();

                try
                {
                    Database = dbScoped.CreateDbContext(ChatService.Connection.ID);

                    while (Database.UnitsLocked)
                    {
                    }

                    Database.UnitsLocked = true;

                    Database.Units.RemoveRange(Database.Units);

                    foreach (var record in units)
                    {
                        var unitInsert = record;

                        unitInsert.Name = $"{record.Name} {CID}";

                        Database.Units.Add(unitInsert);
                    }

                    _ = Database.SaveChangesAsync();

                    Database.UnitsLocked = false;

                    //TRACE
                    Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ ReceiveResponseUnits()");
                    foreach (var record in Database.Units.OrderBy(X => X.ID))
                    {
                        Log.ForContext("Folder", CID).Information($"{record}");
                    }

                    User userLookUp = Database.Users.FirstOrDefault(record => record.ID == LoginService.User.ID);

                    Units = Database.Units.Where(record => record.Agency == userLookUp.Agency).OrderBy(record => record.Agency).ThenBy(record => record.Jurisdiction).ThenBy(record => record.Name).ThenBy(record => record.Status).ThenBy(record => record.Location).AsQueryable().ToList();

                    NotifyStateChangedTableUnits();
                }
                catch (Exception e)
                {
                    Log.ForContext("Folder", CID).Error($"Portal DBScopedService.cs ReceiveResponseUnits() Exception: {e.Message}");
                }
            });

            ChatService.HubConnection.On<Unit?, char?>("ReceiveEventUpdateUnit", (unit, type) =>
            {
                CID = ChatService.Connection.ID.Substring(0, 4).ToUpper();

                //TRACE
                Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ {unit} {type}");

                try
                {
                    Database = dbScoped.CreateDbContext(ChatService.Connection.ID);

                    if (Database == null || !Database.Units.Any())
                    {
                        ChatService.HubConnection.SendAsync("SendRequestUnits");
                    }
                    else
                    {
                        while (Database.UnitsLocked)
                        {
                        }

                        Database.UnitsLocked = true;

                        switch (type)
                        {
                            case 'D':
                                var unitDelete = Database.Units.FirstOrDefault(record => record.ID == unit.ID);

                                if (unitDelete != null)
                                {
                                    Database.Units.Remove(unitDelete);

                                    //TRACE
                                    //Log.ForContext("Folder", CID).Information($"{unitDelete} --> [DELETED]");
                                }

                                break;

                            case 'U':
                                var unitUpdate = Database.Units.FirstOrDefault(record => record.ID == unit.ID);

                                if (unitUpdate != null)
                                {
                                    //TRACE
                                    //Log.ForContext("Folder", CID).Information($"{unitUpdate}");

                                    unitUpdate.Agency = unit.Agency;
                                    unitUpdate.Jurisdiction = unit.Jurisdiction;
                                    unitUpdate.Name = $"{unit.Name} {CID}";
                                    unitUpdate.Status = unit.Status;
                                    unitUpdate.Location = unit.Location;

                                    Database.Units.Update(unitUpdate);

                                    //TRACE
                                    //Log.ForContext("Folder", CID).Information($"{unitUpdate}");
                                }

                                break;

                            case 'I':

                                var unitInsert = unit;

                                unitInsert.Name = $"{unit.Name} {CID}";

                                Database.Units.Add(unitInsert);

                                //TRACE
                                //Log.ForContext("Folder", CID).Information($"{unit} --> [INSERTED]");

                                break;
                        }

                        Database.SaveChangesAsync();

                        Database.UnitsLocked = false;

                        User userLookUp = Database.Users.FirstOrDefault(record => record.ID == LoginService.User.ID);

                        Units = Database.Units.Where(record => record.Agency == userLookUp.Agency).OrderBy(record => record.Agency).ThenBy(record => record.Jurisdiction).ThenBy(record => record.Name).ThenBy(record => record.Status).ThenBy(record => record.Location).AsQueryable().ToList();

                        NotifyStateChangedTableUnits();
                    }
                }
                catch (Exception e)
                {
                    Log.ForContext("Folder", CID).Error($"Portal DBScopedService.cs ReceiveEventUpdateUnit() Exception: {e.Message}");
                }
            });

            ChatService.HubConnection.On<DateTime>("ReceiveServiceActive", (dateTime) =>
            {
                CID = $"{ChatService.Connection.ID.Substring(0, 4).ToUpper()}";

                Log.ForContext("Folder", CID).Information($"------------------------------------------------------------------------------------------ ReceiveServiceActive()");

                _ = SendRequestData();
            });

            _ = SendRequestData();
        }

        public async Task SendRequestData()
        {
            await ChatService.HubConnection.SendAsync("SendRequestUsersLimited");

            await ChatService.HubConnection.SendAsync("SendRequestUnits", LoginService.User);
        }

        private void NotifyStateChangedTableUsers()
        {
            OnChangeTableUsers.Invoke();
        }

        public event Action OnChangeTableUsers;

        private void NotifyStateChangedTableUnits()
        {
            OnChangeTableUnits.Invoke();
        }

        public event Action OnChangeTableUnits;
    }
}
