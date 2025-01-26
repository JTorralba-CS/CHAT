using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

using Standard.Databases;
using Standard.Models;

namespace Service.Services
{
    public class InterfaceService
    {
        public Dictionary<string, DateTime> Connection;

        private int Key;

        private System.Timers.Timer UpdateUsersTimer;

        private int UpdateUsersTimerCount;

        public InterfaceService(int key)
        {
            this.Key = key;

            Connection = new Dictionary<string, DateTime>();

            if (Key == 0)
            {
                UpdateUsersTimer = new System.Timers.Timer(17000);

                UpdateUsersTimerCount = 0;

                UpdateUsersTimer.Elapsed += (s, e) =>
                {
                    if (UpdateUsersTimerCount == 0)
                    {
                        Thread.Sleep(136000);
                    }

                    if (UpdateUsersTimerCount < 10)
                    {
                        UpdateUsers();

                        UpdateUsersTimerCount++;
                    }
                    else
                    {
                        UpdateUsersTimer.Stop();
                    }
                };

                UpdateUsersTimer.Start();
            }
            else
            {
            }
        }

        public List<User> GetUsers()
        {
            using (var tables = new IMDB())
            {
                return tables.Users.OrderBy(user => user.Name).ThenBy(user => user.Password).AsQueryable().ToList();
            }
        }

        public async Task<bool> Authenticate(User user)
        {
            using (var tables = new IMDB())
            {
                var userLookUp = tables.Users.FirstOrDefault(record => record.ID == user.ID && record.Password == user.Password);

                if (userLookUp != null)
                {
                    return await Task.FromResult(true);
                }
                else
                {
                    Log.Information($"Service InterfaceService.cs Authenticate(): User {user.ID} {user.Name} {user.Password} not found.");

                    return await Task.FromResult(false);
                }
            }
        }

		public void DeAuthenticate(User user)
        {
			// Logout code goes here.
		}

        private void UpdateUsers()
        {
            using (var tables = new IMDB())
            {

                Random random = new Random();
                
                // Insert ------------------------------------------------
                
                User userInsert = new User()
                {
                };

                tables.Users.Add(userInsert);

                tables.SaveChangesAsync();

                string First = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                string Last = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                userInsert.Name = $"{Last}, {First} {userInsert.ID.ToString("D6")} {DateTime.Now.ToString("HH:mm:ss")} [Insert]";

                userInsert.Password = $"{userInsert.ID.ToString("D6")}";

                userInsert.Agency = (userInsert.ID / 11) + 1;

                tables.SaveChangesAsync();

                Log.Information($"Service InterfaceService.cs UpdateUsers(): {userInsert.ID} {userInsert.Name} {userInsert.Password} {Key} [Insert]");

                NotifyStateUpdatedUser(userInsert, 'I');

                // Delete ------------------------------------------------

                int ID = random.Next(1, 100);

                var userDelete = tables.Users.FirstOrDefault(record => record.ID == ID);

                if (userDelete != null)
                {
                    tables.Users.Remove(userDelete);
                }

                tables.SaveChangesAsync();

                Log.Information($"Service InterfaceService.cs UpdateUsers(): {userDelete.ID} {userDelete.Name} {userDelete.Password} {Key} [Delete]");

                NotifyStateUpdatedUser(userDelete, 'D');

                // Update ------------------------------------------------

                ID = random.Next(1, 100);

                var userUpdate = tables.Users.FirstOrDefault(record => record.ID == ID);

                First = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                Last = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                userUpdate.Name = $"{Last}, {First} {userUpdate.ID.ToString("D6")} {DateTime.Now.ToString("HH:mm:ss")} [Update]";

                tables.Update(userUpdate);

                tables.SaveChangesAsync();

                Log.Information($"Service InterfaceService.cs UpdateUsers(): {userUpdate.ID} {userUpdate.Name} {userUpdate.Password} {Key} [Update]");

                NotifyStateUpdatedUser(userUpdate, 'U');
            }
        }

        private void NotifyStateUpdatedUser(User user, char type) => OnUpdateUser?.Invoke(user, type);

        public event Action<User, char> OnUpdateUser;
    }
 }
