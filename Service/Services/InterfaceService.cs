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

            for (int i = 1; i <= 1000; i++)
            {
                InterfaceUsers.Add(i,
                    new User {
                        ID = i,
                        Name = $"LAST, USER ({i.ToString("D6")})", Password = i.ToString("D6")
                    });
            }

            GetUsers();
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
