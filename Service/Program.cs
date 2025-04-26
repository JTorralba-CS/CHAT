//OK

// 1) Comment (or remove or try-catch-exception) console input and output interaction.
//
// 2) Do not store console application and/or install service from a OneDrive folder structure.
//
// 3) Run console app as current logged in Windows user.
//
// 4A) Install service as current logged in Windows user with administrative rights (or "Run as administrator").
//
// 4B) Set service "Log On As" account to a specific user (on WORGROUP) or DOMAIN\user (on DOMAIN) that has access to URL/intranet/internet resources.
//
// 4C) Start service.

using Microsoft.Extensions.Configuration;
using System;
using System.Text;

using Topshelf;

using Standard.Functions;
using System.IO;
using System.Reflection;

namespace Service
{
    public class Program
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        static void Main(string[] args)
        {
            try
            {
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                string Title = $"{Configuration["Title"]} (Service)";

                //Windows Services do not like writing to interactive console. Hence, TOPSHELF will fail to install service.
                Console.OutputEncoding = Encoding.UTF8;

                string ChatHub = Configuration["ChatHub"];

                while (ChatHub == null || ChatHub == string.Empty)
                {
                }

                TopshelfExitCode exitCode = HostFactory.Run(x =>
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

                    x.UseSerilog(Core.CreateLogFile("TopShelf"));
                });

                int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());

                Environment.ExitCode = exitCodeValue;
            }
            catch (Exception e)
            {
            }
        }
    }
}
