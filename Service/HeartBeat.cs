using System;
using System.Timers;

using Service.Services;
using Standard.Databases;
using Standard.Models;

namespace Service
{
    public class HeartBeat
    {
        private readonly ChatService ChatService;

        private readonly Timer Timer;

        private const int HeartBeatInterval = 60;

        private DBService DBService;

        public HeartBeat(string chatHub)
        {
            DBService = new DBService();

            using (var Database = DBService.CreateDbContext("System"))
            {
                try
                {
                    for (int i = 1; i <= 100; i++)
                    {
                        string first = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                        string last = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                        string password = $"{last}{first}";

                        var userInsert = new User
                        {
                            Name = $"{last}, {first} {DateTime.Now.ToString("HH:mm:ss")}",
                            Password = password,
                            Agency = (i / 11) + 1
                        };

                        Database.Users.Add(userInsert);
                    }

                    for (int i = 1; i <= 100; i++)
                    {
                        string[] JurisdictionOptions = new[]
                        {
                            "North", "North-East", "East", "South-East", "South", "South-West", "West", "North-West"
                        };

                        int JurisidctionOption = (i / 14);

                        string[] NamePrefixOptions = new[]
                        {
                            "A", "B", "C", "X", "Y", "Z"
                        };

                        int NamePrefixOption = (i / 20);

                        string[] StatusOptions = new[]
                        {
                            "Available", "In Quarters", "Off Duty"
                        };

                        int StatusOption = (i / 50);

                        string[] LocationOptions = new[]
                        {
                            "ABC", "DEF", "GHI", "JKL", "MNO", "PQR", "STU", "VWX"
                        };

                        int LocationOption = (i / 14);

                        Database.Units.Add(
                            new Unit
                            {
                                ID = i,
                                Jurisdiction = JurisdictionOptions[JurisidctionOption],
                                Name = $"{NamePrefixOptions[NamePrefixOption]}{i.ToString("D3")}",
                                Status = StatusOptions[StatusOption],
                                Location = LocationOptions[LocationOption],
                                Agency = (i / 11) + 1
                            });
                    }

                    Database.SaveChangesAsync();

                    //TRACE
                    //foreach (var record in Database.Users)
                    //{
                    //    Console.WriteLine($"{record}");
                    //}

                    //TRACE
                    foreach (var record in Database.Units)
                    {
                        Console.WriteLine($"{record}");
                    }
                }
                catch
                {
                }
            }

            ChatService = new ChatService(chatHub);

            Timer = new Timer(HeartBeatInterval * 1000) { AutoReset = true };
            Timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            Timer.Start();
        }

        public void Stop()
        {
            Timer.Stop();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs a)
        {
            _ = ChatService.Send("❤");
        }
    }
}
