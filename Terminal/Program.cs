//OK

using Standard.Functions;
using Terminal.Services;

namespace Terminal
{
    public class Program
    {
        private static ChatService? ChatService = null;

        static void Main(string[]? args)
        {
            ChatService = new ChatService();

            while (true)
            {
                string? message = Console.ReadLine();

                Core.WriteInfo();

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
