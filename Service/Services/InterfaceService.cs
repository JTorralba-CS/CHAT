using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Standard.Databases;
using Standard.Models;

namespace Service.Services
{
    public class InterfaceService
    {
        public Dictionary<string, DateTime> Connection;

        public List<User> Users;

        public Dictionary<int, User> InterfaceUsers;

        private System.Timers.Timer UpdateUsersTimer;

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

            InitializeInterfaceUsers();

            UpdateUsersTimer = new System.Timers.Timer(5000);

            UpdateUsersTimer.Elapsed += (s, e) =>
            {
                UpdateUsers();

                InitializeInterfaceUsers();

                GetUsers();
            };

            UpdateUsersTimer.Start();
        }

        private void InitializeInterfaceUsers()
        {
            InterfaceUsers.Clear();

            using (var tables = new IMDB())
            {
                foreach (var record in tables.Users)
                {
                    InterfaceUsers.Add(record.ID, record);
                }
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

        private void UpdateUsers()
        {
            using (var tables = new IMDB())
            {

                Random random = new Random();
                
                // Add:
                
                User userAdd = new User()
                {
                };

                tables.Users.Add(userAdd);

                tables.SaveChangesAsync();

                if (userAdd.ID > 175)
                {
                    UpdateUsersTimer.Stop();
                }

                string First = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                string Last = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                userAdd.Name = $"{Last}, {First} {userAdd.ID.ToString("D6")} {DateTime.Now.ToString("HH:mm:ss")} YYY";

                userAdd.Password = $"{userAdd.ID.ToString("D6")}";

                tables.SaveChangesAsync();

                // Delete:

                int ID = random.Next(1, 100);

                var userDelete = tables.Users.FirstOrDefault(record => record.ID == ID);

                if (userDelete != null)
                {
                    tables.Users.Remove(userDelete);
                }

                tables.SaveChangesAsync();

                // Change:

                ID = random.Next(1, 100);

                var userChange = tables.Users.FirstOrDefault(record => record.ID == ID);

                First = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                Last = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                userChange.Name = $"{Last}, {First} {userAdd.ID.ToString("D6")} {DateTime.Now.ToString("HH:mm:ss")} XXX";

                tables.Update(userChange);

                tables.SaveChangesAsync();
            }
        }
    }
 }
