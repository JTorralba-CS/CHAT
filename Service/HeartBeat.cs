//OK

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

        public HeartBeat(string chatHub)
        {
            using (var tables = new IMDB())
            {
                try
                {
                    for (int i = 1; i <= 100; i++)
                    {
                        string First = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                        string Last = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                        tables.Users.Add(
                            new User
                            {
                                ID = i,
                                Name = $"{Last}, {First} {i.ToString("D6")} {DateTime.Now.ToString("HH:mm:ss")}",
                                Password = $"{i.ToString("D6")}"
                            });
                    }
                    tables.SaveChangesAsync();
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
