using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Services
{
    public class InterfaceService
    {
        public List<User> Users;

        public Dictionary<string, DateTime> Connection = new Dictionary<string, DateTime>();

        public InterfaceService(int id)
        {
            Users = new List<User>();

            Users.Add(new User { ID = 1, Name = "Rodrigo, Olivia", Password = "1" });
            Users.Add(new User { ID = 2, Name = "Fran, Andrea", Password = "2" });
            Users.Add(new User { ID = 7, Name = "Pacquiao, Manny", Password = "3" });
            Users.Add(new User { ID = 3, Name = "Magadia, Andrea", Password = "4" });
            Users.Add(new User { ID = 4, Name = "De Lana, Gigi", Password = "5" });
            Users.Add(new User { ID = 5, Name = "Preston, Jeny", Password = "6" });
            Users.Add(new User { ID = 6, Name = "Briggs, Malia", Password = "7" });
            Users.Add(new User { ID = 8, Name = "Lawrence, Jennifer", Password = "8" });
            Users.Add(new User { ID = 9, Name = "Clark, Caitlin", Password = "9" });
            Users.Add(new User { ID = 10, Name = "Houston, Whitney", Password = "10" });
            Users.Add(new User { ID = 17, Name = "Torralba, Julius", Password = "17" });
        }

        public async Task GetUsers()
        {
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public event Action OnChange;
    }

    public class User
    {
        public int ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
            }
        }

        private int _ID { get; set; }

        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
            }
        }

        private string _Name { get; set; }

        public string Password
        {
            get { return _Password; }
            set
            {
                _Password = value;
            }
        }

        private string _Password { get; set; }

        public User()
        {
            _ID = 0;
            _Name = string.Empty;
            _Password = string.Empty;
        }
    }
}
