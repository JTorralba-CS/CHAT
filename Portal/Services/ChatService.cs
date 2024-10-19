using Standard.Models;

namespace Portal.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public ChatService() : base(configuration["ChatHub"])
        {
            Connection.Alias = "Portal";
        }
    }
}
