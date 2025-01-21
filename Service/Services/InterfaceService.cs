using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Standard.Models;

namespace Service.Services
{
    public class InterfaceService
    {
        public Dictionary<string, DateTime> Connection;

        public List<User> Users;

        public Dictionary<int, User> InterfaceUsers;

        public InterfaceService()
        {
            Connection = new Dictionary<string, DateTime>();

            Users = new List<User>();

            InterfaceUsers = new Dictionary<int, User>();

            //InterfaceUsers.Add(1, new User { ID = 1, Name = "Rodrigo, Olivia", Password = "1" });
            //InterfaceUsers.Add(2, new User { ID = 2, Name = "Fran, Andrea", Password = "2" });
            //InterfaceUsers.Add(7, new User { ID = 7, Name = "Pacquiao, Manny", Password = "7" });
            //InterfaceUsers.Add(3, new User { ID = 3, Name = "Magadia, Andrea", Password = "3" });
            //InterfaceUsers.Add(4, new User { ID = 4, Name = "De Lana, Gigi", Password = "4" });
            //InterfaceUsers.Add(5, new User { ID = 5, Name = "Preston, Jeny", Password = "5" });
            //InterfaceUsers.Add(6, new User { ID = 6, Name = "Briggs, Malia", Password = "6" });
            //InterfaceUsers.Add(8, new User { ID = 8, Name = "Lawrence, Jennifer", Password = "8" });
            //InterfaceUsers.Add(9, new User { ID = 9, Name = "Clark, Caitlin", Password = "9" });
            //InterfaceUsers.Add(10, new User { ID = 10, Name = "Houston, Whitney", Password = "10" });
            //InterfaceUsers.Add(17, new User { ID = 17, Name = "Torralba, Julius", Password = "17" });

            InitializeUsers();
        }

        private void InitializeUsers()
        {
            InterfaceUsers.Clear();

            for (int i = 1; i <= 1200; i++)
            {
                string First = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                string Last = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                InterfaceUsers.Add(i,
                    new User
                    {
                        ID = i,
                        Name = $"{Last}, {First} {i.ToString("D6")} {DateTime.Now.ToString("HH:mm:ss")}",
                        Password = $"{i.ToString("D6")}"
                    });
            }
        }

        public void GetUsers()
        {
            Users.Clear();

            foreach (var record in InterfaceUsers)
            {
                Users.Add(record.Value);
            }

            NotifyStateChangedUsers();
        }

        public async Task<bool> Authenticate(User user)
        {
            if (InterfaceUsers[user.ID].Password == user.Password)
            {
                return await Task.FromResult(true);
            }
            else
            {
                return await Task.FromResult(false);
            }
        }

		public void DeAuthenticate(User user)
        {
			// Logout code goes here.
		}

		private void NotifyStateChangedUsers() => OnChangeUsers?.Invoke();

        public event Action OnChangeUsers;
    }
 }
