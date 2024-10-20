using Terminal.Services;

namespace Terminal
{
    public class Program
    {
        private static ChatService ChatService;

        static void Main(string[] args)
        {
            ChatService = new ChatService();

            while (true)
            {
                var message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    break;
                }

                _ = ChatService.Send(message);
            }

            Console.ReadLine();
        }
    }
}
