namespace Portal.Services
{
    public class ChatService : Standard.Services.ChatService
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public ChatService(StateService stateService) : base(Configuration["ChatHub"])
        {
            if (stateService.IsInitialPortal)
            {
                stateService.UnSetIsInitialPortal();

                Connection.Alias = "PORTAL";

                _ = SetAlias("PORTAL");
            }
            else
            {
                Connection.Alias = "PORTALX";

                _ = SetAlias("PORTALX");
            }           
        }
    }
}
