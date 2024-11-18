using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

using Standard.Functions;
using Standard.Models;

namespace Service.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        private Dictionary<int, InterfaceService> InterfaceInstance;

        private static int DEBUG = Core.DEBUG;

        private DateTime RecieveServiceActiveDateTime;

        public ChatService(string chatHub) : base(chatHub)
        {
            string Title = Configuration["Title"];

            InterfaceInstance = new Dictionary<int, InterfaceService>();

            CreateInterfaceInstance(Connection, new User { ID = 0, Name = Title, Password = string.Empty });

            try
            {
                Connection.Alias = Title;

                HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
                {
                    if (connection.ID == HubConnection.ConnectionId || connection.Alias == Connection.Alias)
                    {
                        Core.WriteConsole($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        Core.WriteConsole($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                    }

                    if (message == "_" || message == "❤")
                    {
                        ConnectionMaintenance(connection);
                    }
                });

                HubConnection.On("ReceiveRequestUsers", () =>
                {
                    InterfaceInstance[0].GetUsers();

                    ConnectionMaintenance();
                });

                HubConnection.On<Connection, User>("ReceiveRequestLogin", (connection, user) =>
                {
                    CreateInterfaceInstance(connection, user);

                    HubConnection.SendAsync("SendResponseLogin", connection, user, InterfaceInstance[user.ID].Authenticate(user).Result);

                    ConnectionMaintenance();
                });

                HubConnection.On<Connection, Standard.Models.User>("ReceiveRequestLogout", (connection, user) =>
                {
                    if (DEBUG == 2)
                    {
                        Core.WriteInfo($"Service ChatService.cs ReceiveRequestLogout(): {connection.ID} {connection.Alias} {user.ID} {user.Name}");
                    }

                    try
                    {

                        if (InterfaceInstance[user.ID].Connection.Remove(connection.ID))
                        {
                            if (DEBUG == 2)
                            {
                                Core.WriteInfo($"InterfaceInstance[{user.ID}] {user.Name} has {InterfaceInstance[user.ID].Connection.Count} connection IDs.");
                            }

                            if (InterfaceInstance[user.ID].Connection.Count == 0)
                            {
                                InterfaceInstance.Remove(user.ID);

                                if (DEBUG == 2)
                                {
                                    Core.WriteInfo($"InterfaceInstance[{user.ID}] {user.Name} destroyed.");
                                }
                            }
                            else
                            {
                                if (DEBUG == 2)
                                {
                                    Core.WriteInfo($"InterfaceInstance[{user.ID}] {user.Name} still has connections and/or could not be destroyed.");
                                }
                            }
                        }

                        if (DEBUG == 2)
                        {
                            Core.WriteInfo($"InterfaceInstance(s) = {InterfaceInstance.Count}.");
                        }
                    }
                    catch (Exception e)
                    {
                        if (DEBUG == 1)
                        {
                            Core.WriteError($"Service ChatService.cs ReceiveRequestLogout() Exception: {connection.ID} {connection.Alias} {user.ID} {user.Name} {e.Message}");
                        }
                    }
                    finally
                    {
                        if (DEBUG == 2)
                        {
                            Core.WriteInfo($"Service ChatService.cs ReceiveRequestLogout() Finally: {connection.ID} {connection.Alias} {user.ID} {user.Name}");
                        }
                        
                        HubConnection.SendAsync("SendResponseLogout", connection);
                    }

                    ConnectionMaintenance();
                });

                HubConnection.On<DateTime>("ReceiveServiceActive", (dateTime) =>
                {
                    RecieveServiceActiveDateTime = dateTime;

                    if (DEBUG == 2)
                    {
                        Core.WriteInfo($"Service ChatService.cs ReceiveServiceActive(): {dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                    }

                    ConnectionMaintenance();
                });
            }
            catch (Exception e)
            {
                if (DEBUG == 1)
                {
                    Core.WriteError($"Service ChatService.cs ChatService() Exception: {e.Message}");
                }
            }
        }
        private void CreateInterfaceInstance(Connection connection, User user)
        {
            InterfaceService interfaceService;

            int key = user.ID;

            if (InterfaceInstance.TryGetValue(key, out interfaceService))
            {
                if (interfaceService != null)
                {
                    DateTime connectionTimeStamp;

                    string connectionKey = connection.ID;

                    if (InterfaceInstance[key].Connection.TryGetValue(connectionKey, out connectionTimeStamp))
                    {
                        if (connectionTimeStamp != null)
                        {
                            InterfaceInstance[key].Connection[connectionKey] = DateTime.Now;
                        }
                    }
                    else
                    {
                        InterfaceInstance[key].Connection.Add(connectionKey, DateTime.Now);
                    }
                }
            }
            else
            {
                try
                {
                    InterfaceInstance.Add(key, new InterfaceService());

                    string connectionKey = connection.ID;
                    
                    InterfaceInstance[key].Connection.Add(connectionKey, DateTime.Now);
                }
                catch (Exception e)
                {
                    if (DEBUG == 1)
                    {
                        Core.WriteError($"Service ChatService.cs CreateInterfaceInstance() Exception: {e.Message}");
                    }
                }

                InterfaceInstance[key].OnChangeUsers += () =>
                {
                    HubConnection.SendAsync("SendResponseUsers", InterfaceInstance[0].Users.OrderBy(_user => _user.Name.ToUpper().Trim().Replace("  ", " ").Replace("  ", " ")).ToList());
                };
            }
        }

        public void ConnectionMaintenance()
        {
            var InterfaceInstanceSorted = InterfaceInstance.OrderBy(x => x.Key);

            foreach (KeyValuePair<int, InterfaceService> entry in InterfaceInstanceSorted)
            {
                if (entry.Key != 0)
                {
                    //Core.WriteInfo($"InterfaceInstance[{entry.Key}] {entry.Value.Connection.Count} connections:");

                    if (entry.Value.Connection.Count > 0)
                    {
                        foreach (KeyValuePair<string, DateTime> entry2 in entry.Value.Connection)
                        {
                            var orphan = false;

                            if (entry2.Value < RecieveServiceActiveDateTime)
                            {
                                orphan = true;
                            }

                            if (DEBUG == 2)
                            {
                                //Core.WriteInfo($"\tConnection[{entry2.Key}] {entry2.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")} < {RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")} ({orphan})");
                            }

                            if (orphan)
                            {
                                InterfaceInstance[entry.Key].Connection.Remove(entry2.Key);
                            }
                        }
                    }
                    else
                    {
                        InterfaceInstance.Remove(entry.Key);
                    }
                }
            }
        }

        public void ConnectionMaintenance(Connection connection)
        {
            var InterfaceInstanceSorted = InterfaceInstance.OrderBy(x => x.Key);

            foreach (KeyValuePair<int, InterfaceService> entry in InterfaceInstanceSorted)
            {
                if (entry.Key != 0)
                {
                    //Core.WriteInfo($"InterfaceInstance[{entry.Key}] {entry.Value.Connection.Count} connections:");

                    HubConnection.SendAsync("SendMessageToSender", connection, $"InterfaceInstance[{entry.Key}] {entry.Value.Connection.Count} connections:");

                    if (entry.Value.Connection.Count > 0)
                    {
                        foreach (KeyValuePair<string, DateTime> entry2 in entry.Value.Connection)
                        {
                            var orphan = false;

                            if (entry2.Value < RecieveServiceActiveDateTime)
                            {
                                orphan = true;
                            }

                            //Core.WriteInfo($"\tConnection[{entry2.Key}] {entry2.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")} < {RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")} ({orphan})");

                            HubConnection.SendAsync("SendMessageToSender", connection, $"\tConnection[{entry2.Key}] {entry2.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")} < {RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")} ({orphan})");
                        }
                    }
                }
            }
        }

    }
}
