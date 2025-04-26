//OK

using System.Reflection;

using Serilog;

using Standard.Functions;
using Terminal.Services;

namespace Terminal
{
    public class Program
    {
        private static ChatService? ChatService = null;

        static void Main(string[]? args)
        {
            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                Log.Logger = Core.CreateLogFile("Terminal");
                //Log.Logger = Core.CreateLog();

                ChatService = new ChatService();

                while (true)
                {
                    string? message = Console.ReadLine();

                    if (string.IsNullOrEmpty(message))
                    {
                        break;
                    }

                    _ = ChatService.Send(message);
                }

                Console.ReadLine();
            }
            catch (Exception e)
            {
            }
        }
    }
}
