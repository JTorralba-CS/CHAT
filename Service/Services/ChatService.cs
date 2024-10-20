using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

using Standard.Models;

namespace Service.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        private Dictionary<int, InterfaceService> InterfaceInstance;

        private static bool DEBUG = true;

        public ChatService() : base(Configuration["ChatHub"])
        {
            InterfaceInstance = new Dictionary<int, InterfaceService>();

            CreateInterfaceInstance(Connection, new Standard.Models.User { ID = 0, Name = "SERVICE" });

            try
            {
                Connection.Alias = "Service";

                HubConnection.On<Connection, string>("ReceiveMessage", (connection, message) =>
                {
                    if (DEBUG)
                    {
                        //Console.WriteLine();
                        //Console.WriteLine($"Service ChatService.cs ReceiveMessage {HubConnection.ConnectionId} {connection.ID} {connection.Alias} {message}");
                    }

                    if (connection.ID == HubConnection.ConnectionId || connection.Alias == Connection.Alias)
                    {
                        Log($"{connection.Alias}: {message}", ConsoleColor.Cyan);
                    }
                    else
                    {
                        Log($"{connection.Alias}: {message}", ConsoleColor.Magenta);
                    }
                });

                HubConnection.On<Connection>("ReceiveRequestUsers", (connection) =>
                {
                    _ = InterfaceInstance[0].GetUsers();
                });
            }
            catch (Exception e)
            {
                if (DEBUG)
                {
                    Console.WriteLine($"Serivce ChatService.cs ChatService(): {e.Message}");
                }
            }
        }
        private void CreateInterfaceInstance(Connection connection, Standard.Models.User user)
        {
            InterfaceService interfaceService;

            int key = user.ID;

            if (InterfaceInstance.TryGetValue(key, out interfaceService))
            {
                if (interfaceService != null)
                {
                    if (DEBUG)
                    {
                        Console.WriteLine($"InterfaceInstance[{key}] {user.Name} already exists.");
                    }

                    DateTime connectionTimeStamp;

                    string connectionKey = connection.ID;

                    if (InterfaceInstance[key].Connection.TryGetValue(connectionKey, out connectionTimeStamp))
                    {
                        if (connectionTimeStamp != null)
                        {
                            if (DEBUG)
                            {
                                Console.WriteLine($"InterfaceInstance[{key}].Connection[{connectionKey}] {connection.Alias} already exists.");
                            }
                        }
                    }
                    else
                    {
                        InterfaceInstance[key].Connection.Add(connectionKey, DateTime.Now);

                        if (DEBUG)
                        {
                            Console.WriteLine($"InterfaceInstance[{key}] has {InterfaceInstance[key].Connection.Count} connection(s).");
                        }
                    }
                }
            }
            else
            {
                try
                {
                    InterfaceInstance.Add(key, new InterfaceService(key));

                    DateTime connectionTimeStamp;

                    string connectionKey = connection.ID;

                    if (InterfaceInstance[key].Connection.TryGetValue(connectionKey, out connectionTimeStamp))
                    {
                        if (connectionTimeStamp != null)
                        {
                            if (DEBUG)
                            {
                                Console.WriteLine($"InterfaceInstance[{key}].Connection[{connectionKey}] {connection.Alias} already exists.");
                            }
                        }
                    }
                    else
                    {
                        InterfaceInstance[key].Connection.Add(connectionKey, DateTime.Now);

                        if (DEBUG)
                        {
                            Console.WriteLine($"InterfaceInstance[{key}] has {InterfaceInstance[key].Connection.Count} connection(s).");
                        }
                    }
                }
                catch (Exception e)
                {
                    if (DEBUG)
                    {
                        Console.WriteLine($"Service ChatService.cs CreateInterfaceInstance(): {e.Message}");
                    }
                }

                InterfaceInstance[key].OnChange += () =>
                {
                    List<Standard.Models.User> users = new List<Standard.Models.User>();

                    foreach (var item in InterfaceInstance[0].Users)
                    {
                        users.Add(new Standard.Models.User { ID = item.ID, Name = item.Name });
                    }

                    HubConnection.SendAsync("SendResponseUsers", connection, users);
                };

                if (DEBUG)
                {
                    Console.WriteLine($"InterfaceInstance[{key}] {user.Name} created.");
                }
            }
        }

        public static void Log(string message, ConsoleColor consoleColor = ConsoleColor.White)
        {
            if (DEBUG)
            {
                Console.ForegroundColor = consoleColor;

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} {message}");

                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
