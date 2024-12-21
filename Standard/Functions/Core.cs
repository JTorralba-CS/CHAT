//OK

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using Serilog;

namespace Standard.Functions
{
    public static class Core
    {
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
                //Windows Services do not like writing to interactive console. Hence, TOPSHELF process will fail to install service application.

                if (message != string.Empty)
                {
                    Console.ForegroundColor = consoleColor;
                    Console.WriteLine($"[INF] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}");
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
            }
            catch (Exception e)
            {
            }
        }

        public static ILogger CreateLogFile(string assemblyName = "")
        {
            return new LoggerConfiguration()
                .WriteTo.File($"logs\\{assemblyName}\\.log",
                    Serilog.Events.LogEventLevel.Verbose,
                    rollingInterval: SeriLog.RollingInterval,
                    retainedFileTimeLimit: TimeSpan.FromMinutes(5),
                    shared: true,
                    outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}"
                )
                .CreateLogger();
        }

        public static ILogger CreateLog()
        {
            var Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Standard.json")
                .Build();

            return new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.Map("Folder", "System", (Folder, WT) =>
                    WT.File($"logs\\{Folder}\\.log",
                    rollingInterval: SeriLog.RollingInterval,
                    retainedFileTimeLimit: TimeSpan.Parse(SeriLog.RetainedFileTimeLimit),
                    shared: true,
                    outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}"
                    )
                )
                .CreateLogger();
        }
    }

    public static class SeriLog
    {

        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("Standard.json").Build();

        public static readonly string RetainedFileTimeLimit = (Configuration["SeriLog:RetainedFileTimeLimit"]);

        public static readonly Serilog.RollingInterval RollingInterval = (Serilog.RollingInterval) Enum.Parse(typeof(Serilog.RollingInterval), Configuration["SeriLog:RollingInterval"], true);

        public static readonly string NextLine = '\n' + new string (' ', 30);

        public static string Format(string method = "[n/a]", string from = "[n/a]", string to = "[n/a]", string message = "[n/a]")
        {
            return string.Concat(method, NextLine, from, NextLine, to, NextLine, message);
        }

        public static readonly string EXE_Path = Path.GetDirectoryName(AppContext.BaseDirectory);
    }

    public static class EventViewer
    {
        public static void Information(string source, int eventID, string general)
        {
            Log(EventLogEntryType.Information, source, eventID, general);
        }

        public static void Error(string source, int eventID, string general)
        {
            Log(EventLogEntryType.Error, source, eventID, general);
        }

        public static void Log(EventLogEntryType level, string source, int eventID, string general)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = source;
                eventLog.WriteEntry(general, level, eventID, 0);
            }
        }
    }
}
