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

                    Database.SaveChangesAsync();

                    //TRACE
                    //foreach (var record in Database.Users)
                    //{
                    //    Console.WriteLine($"{record}");
                    //}
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
