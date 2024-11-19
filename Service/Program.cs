//OK

// 1) Comment (or remove) console input and output interaction.
//
// 2) Do not store console application and/or install service from a OneDrive folder structure.
//
// 3) Set service "Log On As" account to a specific domain user or domain admin user or local user that has access to URL/intranet/internet resources.
//
// 4) Run console app as current logged in Windows user.
//
// 5A) Install service as current logged in Windows user.
//
// 5B) Set service "Log On" properties as current logged in Windows user.
//
// 5C) Start service as current logged in Windows user.

using Microsoft.Extensions.Configuration;
using System;
using System.Text;

using Serilog;
using Topshelf;
using System.Diagnostics;

//using Standard.Functions;

namespace Service
{
    public class Program
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        static void Main(string[] args)
        {
            string Title = $"{Configuration["Title"]} (Service)";

            try
            {
                //Windows Services do not like writing to interactive console. Hence, TOPSHELF will fail to install service.
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch (Exception e)
            {
            }

            string ChatHub = Configuration["ChatHub"];

            while (ChatHub == null || ChatHub == string.Empty)
            {
            }

            var exitCode = HostFactory.Run(x =>
            {
                x.Service<HeartBeat>(s =>
                {
                    s.ConstructUsing(heartBeat => new HeartBeat(ChatHub));
                    s.WhenStarted(heartBeat => heartBeat.Start());
                    s.WhenStopped(heartBeat => heartBeat.Stop());
                });

                x.SetServiceName(Title);
                x.SetDisplayName(Title);
                x.SetDescription(Title);

                x.UseSerilog(CreateLogger());
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());

            Environment.ExitCode = exitCodeValue;
        }

        private static Serilog.ILogger CreateLogger()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.File("_TopShelf.txt", Serilog.Events.LogEventLevel.Debug)
                .CreateLogger();
            return logger;
        }
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
