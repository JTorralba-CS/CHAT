using Standard.Models;

namespace Portal.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public ChatService() : base(Configuration["ChatHub"])
        {
            Connection.Alias = "Portal";
        }
    }
}
