//OK

// 1) Do not store console application and/or install service from a OneDrive folder structure.
// 2) Set service "Log On As" account to a specific domain user or domain admin user or local user that has access to URL/intranet/internet resources.

using Microsoft.Extensions.Configuration;
using System;
using System.Text;

using Topshelf;

namespace Service
{
    public class Program
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        static void Main(string[] args)
        {
            string Title = $"{Configuration["Title"]} (Service)";

            Console.OutputEncoding = Encoding.UTF8;

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
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());

            Environment.ExitCode = exitCodeValue;
        }
    }
}
