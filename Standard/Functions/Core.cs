//OK

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Standard.Functions
{
    public static class Core
    {
        // 0 = None
        // 1 = Exception
        // 2 = Info

        public static int DEBUG = 2;

        public static string[] SplitSpaceInput(string input)
        {
            Regex Pattern = new Regex("(?:^| )(\"(?:[^\"]+|\"\")*\"|[^ ]*)", RegexOptions.Compiled);

            List<string> list = new List<string>();

            string current;

            foreach (Match match in Pattern.Matches(input))
            {
                current = match.Value;
                if (0 == current.Length)
                {
                    list.Add("");
                }

                list.Add(current.TrimStart(' ').TrimStart('"').TrimEnd('"').Trim());
            }

            return list.ToArray();
        }

        public static string[] SplitCommaInput(string input)
        {
            Regex Pattern = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);

            List<string> list = new List<string>();

            string current;

            foreach (Match match in Pattern.Matches(input))
            {
                current = match.Value;
                if (0 == current.Length)
                {
                    list.Add("");
                }

                list.Add(current.TrimStart(',').TrimStart('"').TrimEnd('"').Trim());
            }

            return list.ToArray();
        }

        public static void WriteInfo()
        {
            WriteConsole(string.Empty);
        }

        public static void WriteInfo(string message)
        {
            WriteConsole(message);
        }

        public static void WriteError(string message)
        {
            WriteConsole(message, ConsoleColor.Red);
        }

        public static void WriteConsole(string message, ConsoleColor consoleColor = ConsoleColor.White)
        {
            try
            {
                //Windows Services do not like writing to interactive console. Hence, TOPSHELF will fail to install service.
                if (message != string.Empty)
                {
                    Console.ForegroundColor = consoleColor;
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss} {message}");
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
            }
            catch (Exception e)
            {
            }
        }
    }
}
