//OK

using System.Timers;

using Service.Services;

namespace Service
{
    public class HeartBeat
    {
        private readonly ChatService ChatService;

        private readonly Timer Timer;

        private const int HeartBeatInterval = 60;

        public HeartBeat(string chatHub)
        {
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
