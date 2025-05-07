using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

using Serilog;

using Standard.Functions;
using Standard.Models;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        private Dictionary<int, InterfaceService> InterfaceInstance;

        private DateTime RecieveServiceActiveDateTime;

        private string Title;

        public ChatService(string chatHub) : base(chatHub)
        {
            Log.Logger = Core.CreateLogFile("Service");

            Title = Configuration["Title"];

            InterfaceInstance = new Dictionary<int, InterfaceService>();

            try
            {
                Connection.Alias = Title.ToUpper();

                HubConnection.On<Connection, User, string>("ReceiveMessage", (connection, user, message) =>
                {
                    if (message.ToUpper().Contains("REDLIGHT"))
                    {
                        message = Regex.Replace(message, "REDLIGHT", "", RegexOptions.IgnoreCase).Trim();

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
                            _ = HubConnection.SendAsync("SendServiceActive");

                            ConnectionStatus(connection, user);
                        }

                        Log.Information($"{connection.Alias} (User ID = {user.ID}): {message}");

                        string Result = InterfaceInstance[user.ID].Command(message.ToLower()).Trim();

                        if (Result != string.Empty)
                        {
                            //TRACE
                            //Log.Information($"Service ChatService.cs ReceiveMessage(): {Result} [RESULT]");

                            HubConnection.SendAsync("SendMessagePrivate", connection, user, Result, Connection);
                        }
                    }
                });

                HubConnection.On<Connection, User>("ReceiveRequestLogin", (connection, user) =>
                {
                    CreateInterfaceInstance(connection, user);

                    var Authenticated = InterfaceInstance[user.ID].Authenticate(user).Result;

                    if (Authenticated)
                    {
                        // Prevent concurrent login.
                        if (InterfaceInstance.TryGetValue(user.ID, out InterfaceService interfaceService))
                        {
                                try
                                {
                                    foreach (string key in interfaceService.Connection.Keys)
                                    {
                                        if (key != connection.ID)
                                        {
                                            HubConnection.SendAsync("SendRequestLogout", new Connection { ID = key, Alias = user.Name }, user);
                                            HubConnection.SendAsync("SendResponseLogout", new Connection { ID = user.Name, Alias = user.Name });
                                        }
                                    }

                                }
                                catch (Exception e)
                                {
                                    Log.Error($"Service ChatService.cs ChatService() ReceiveRequestLogin() Exception: {e.Message}");
                                }
                        }
                    }

                    _ = HubConnection.SendAsync("SendResponseLogin", connection, user, Authenticated);

                    ConnectionMaintenance();
                });

                HubConnection.On<Connection, User>("ReceiveRequestLogout", (connection, user) =>
                {
                    try
                    {
                        InterfaceInstance[user.ID].DeAuthenticate(user);

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
                        Core.WriteError($"Service ChatService.cs ChatService() ReceiveRequestLogout() Exception: {connection.ID} {connection.Alias} {user.ID} {user.Name} {e.Message}");

                        Log.Error($"Service ChatService.cs ChatService() ReceiveRequestLogout() Exception: {connection.ID} {connection.Alias} {user.ID} {user.Name} {e.Message}");
                    }
                    finally
                    {
                        _ = HubConnection.SendAsync("SendResponseLogout", connection);
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

                //VALID ----------------------------------------------------------------------------------------------------
                HubConnection.On<string>("ReceiveRequestUsersLimited", (connectionID) =>
                {
                    _ = HubConnection.SendAsync("SendResponseUsersLimited", connectionID, InterfaceInstance[0].GetUsers());

                    ConnectionMaintenance();
                });

                HubConnection.On<string, User>("ReceiveRequestUnits", (connectionID, user) =>
                {
                    _ = HubConnection.SendAsync("SendResponseUnits", connectionID, InterfaceInstance[user.ID].GetUnits());

                    ConnectionMaintenance();
                });
            }
            catch (Exception e)
            {
                Core.WriteError($"Service ChatService.cs ChatService() Exception: {e.Message}");

                Log.Error($"Service ChatService.cs ChatService() Exception: {e.Message}");
            }

            CreateInterfaceInstance(Connection, new User { ID = 0, Name = Title, Password = string.Empty });

            _ = HubConnection.SendAsync("SendServiceActive");
        }

        public async Task Send(string message)
        {
            await Send(new User() { Name = Title }, message);
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
                    InterfaceInstance.Add(key, new InterfaceService(key));

                    string connectionKey = connection.ID;

                    InterfaceInstance[key].Connection.Add(connectionKey, DateTime.Now);

                    InterfaceInstance[key].OnUpdateUser += (updateUser, updateType) =>
                    {
                        if (key == 0)
                        {
                            //TRACE
                            Log.Information($"Service ChatService.cs CreateInterfaceInstance() OnUpdateUser(): {updateUser} {updateType} {connection}");
                            HubConnection.SendAsync("SendEventUpdateUser", updateUser, updateType);
                        }
                    };

                    InterfaceInstance[key].OnUpdateUnit += (updateUnit, updateType) =>
                    {
                        if (key == 0)
                        {
                            //TRACE
                            Log.Information($"Service ChatService.cs CreateInterfaceInstance() OnUpdateUnit(): {updateUnit} {updateType} {connection}");
                            HubConnection.SendAsync("SendEventUpdateUnit", updateUnit, updateType);
                        }
                    };
                }
                catch (Exception e)
                {
                    Core.WriteError($"Service ChatService.cs CreateInterfaceInstance() Exception: {e.Message}");

                    Log.Error($"Service ChatService.cs CreateInterfaceInstance() Exception: {e.Message}");
                }
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

        public void ConnectionStatus(Connection connection, User user)
        {
            var InterfaceInstanceSorted = InterfaceInstance.OrderBy(x => x.Key);

            foreach (KeyValuePair<int, InterfaceService> entry in InterfaceInstanceSorted)
            {
                if (entry.Key != 0)
                {
                    //HubConnection.SendAsync("SendMessagePrivate", connection, $"InterfaceInstance[{entry.Key}] {entry.Value.Connection.Count} connections:", Connection);

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

                            //HubConnection.SendAsync("SendMessagePrivate", connection, $"\tConnection[{entry2.Key}] {entry2.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")} < {RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")} ({orphan})", Connection);

                            X = X + SeriLog.NextLine + $"Connection[{entry2.Key}] {entry2.Value.ToString("yyyy-MM-dd HH:mm:ss.fff")} < {RecieveServiceActiveDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")} ({orphan})";
                        }
                    }

                    HubConnection.SendAsync("SendMessagePrivate", connection, user, X, Connection);
                }
            }
        }
    }
}
