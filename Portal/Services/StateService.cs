//OK

namespace Portal.Services
{
    public class StateService
    {
        public bool IsInitialPortal => _IsInitialPortal;

        private bool _IsInitialPortal = true;

        public bool IsInitialService => _IsInitialService;

        private bool _IsInitialService = true;

        public StateService()
        {
        }

        public void UnSetIsInitialPortal()
        {
            _IsInitialPortal = false;
        }

        public void UnSetIsInitialService()
        {
            _IsInitialPortal = false;
        }
    }
}
