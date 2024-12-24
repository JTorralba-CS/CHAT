using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

using Serilog;

using Standard.Functions;
using Standard.Models;

namespace Service.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        private Dictionary<int, InterfaceService> InterfaceInstance;

        private DateTime RecieveServiceActiveDateTime;

        public ChatService(string chatHub) : base(chatHub)
        {
            Log.Logger = Core.CreateLogFile("Service");

            string Title = Configuration["Title"];

            InterfaceInstance = new Dictionary<int, InterfaceService>();

            CreateInterfaceInstance(Connection, new User { ID = 0, Name = Title, Password = string.Empty });

            try
            {
                Connection.Alias = Title.ToUpper();

                HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
                {
                    if (message.Contains("!!!"))
                    {
                        message = message.Replace("!!!", "").Trim();

                        if (message == string.Empty)
                        {
                            message = "We apologize for the inconvenience. System offline for maintanence. Current session(s) may expire or disconnect. Refresh browser and/or login at your later convenience.";
                        }

                        Core.WriteConsole($"{connection.Alias}: {message} [notification]", ConsoleColor.Red);

                        Log.Information($"{connection.Alias}: {message} [notification]");
                    }
                    else
                    {
                        if (connection.ID == HubConnection.ConnectionId || connection.Alias.ToUpper() == Connection.Alias.ToUpper())
                        {
                            Core.WriteConsole($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                        }
                        else
                        {
                            Core.WriteConsole($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                        }

                        if (message == "_" || message == "❤")
                        {
                            HubConnection.SendAsync("SendServiceActive");

                            ConnectionMaintenance(connection);
                        }

                        Log.Information($"{connection.Alias}: {message}");
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
                    try
                    {

                        if (InterfaceInstance[user.ID].Connection.Remove(connection.ID))
                        {
                            if (InterfaceInstance[user.ID].Connection.Count == 0)
                            {
                                InterfaceInstance.Remove(user.ID);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"ReceiveRequestLogout() Exception: {connection.ID} {connection.Alias} {user.ID} {user.Name} {e.Message}");

                        Core.WriteError($"Service ChatService.cs ReceiveRequestLogout() Exception: {connection.ID} {connection.Alias} {user.ID} {user.Name} {e.Message}");
                    }
                    finally
                    {
                        HubConnection.SendAsync("SendResponseLogout", connection);
                    }

                    ConnectionMaintenance();
                });

                HubConnection.On<DateTime>("ReceiveServiceActive", (dateTime) =>
                {
                    RecieveServiceActiveDateTime = DateTime.Now;

                    ConnectionMaintenance();
                });

                HubConnection.On<Connection>("ReceiveConnected", (connection) =>
                {
                    Core.WriteInfo($"ReceiveConnected {connection.ID} <{Connection.Alias}>");

                    Log.Information($"ReceiveConnected {connection.ID} <{Connection.Alias}>");

                    Connection.ID = connection.ID;

                    _ = SetAlias(Connection.Alias);
                });
            }
            catch (Exception e)
            {
                Log.Error($"Service ChatService.cs ChatService() Exception: {e.Message}");

                Core.WriteError($"Service ChatService.cs ChatService() Exception: {e.Message}");
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
                    Log.Error($"Service ChatService.cs CreateInterfaceInstance() Exception: {e.Message}");

                    Core.WriteError($"Service ChatService.cs CreateInterfaceInstance() Exception: {e.Message}");
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
                    if (entry.Value.Connection.Count > 0)
                    {
                        foreach (KeyValuePair<string, DateTime> entry2 in entry.Value.Connection)
                        {
                            bool orphan = false;

                            if (entry2.Value < RecieveServiceActiveDateTime)
                            {
                                orphan = true;
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
                    //HubConnection.SendAsync("SendMessageToSender", connection, $"InterfaceInstance[{entry.Key}] {entry.Value.Connection.Count} connections:");

                    var X = $"InterfaceInstance[{entry.Key}] {entry.Value.Connection.Count} connections:";

                    if (entry.Value.Connection.Count > 0)
                    {
                        foreach (KeyValuePair<string, DateTime> entry2 in entry.Value.Connection)
                        {
                            bool orphan = false;

                            if (entry2.Value < RecieveServiceActiveDateTime)
                            {
                                orphan = true;
                            }

                            //HubConnection.SendAsync("SendMessageToSender", connection, $"\tConnection[{entry2.Key}] {entry2.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")} < {RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")} ({orphan})");

                            X = X + SeriLog.NextLine + $"Connection[{entry2.Key}] {entry2.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")} < {RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")} ({orphan})";
                        }
                    }

                    HubConnection.SendAsync("SendMessageToSender", connection, X);
                }
            }
        }

    }
}
