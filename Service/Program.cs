using System;

using Topshelf;

namespace Service
{
    public class Program
    {
        static void Main(string[] args)
        {

            // 1) Do not store console application and/or install service from a OneDrive folder structure.
            // 2) Set service "Log On As" account to a specific domain user or domain admin user that has access to URL/intranet/internet resources.

            var exitCode = HostFactory.Run(x =>
            {
                x.Service<HeartBeat>(s =>
                {
                    s.ConstructUsing(heartBeat => new HeartBeat());
                    s.WhenStarted(heartBeat => heartBeat.Start());
                    s.WhenStopped(heartBeat => heartBeat.Stop());
                });

                x.SetServiceName("Chat");
                x.SetDisplayName("Chat");
                x.SetDescription("Chat service.");
            });

            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());

            Environment.ExitCode = exitCodeValue;
        }
    }
}
