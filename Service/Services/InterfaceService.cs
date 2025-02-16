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

        private static IMDB tables;

        public InterfaceService(int key)
        {
            this.Key = key;

            Connection = new Dictionary<string, DateTime>();

            if (Key == 0)
            {
                if (tables == null)
                {
                    Log.Information($"Service InterfaceService.cs InterfaceService(): tables == null");

                    tables = new IMDB();
                }

                tables.OnChangeTable += (user, type) =>
                {
                    NotifyStateUpdatedUser(user, type);
                };

                UpdateUsersTimer = new System.Timers.Timer(10000);

                UpdateUsersTimerCount = 0;           

                UpdateUsersTimer.Elapsed += (s, e) =>
                {
                    if (UpdateUsersTimerCount == 0)
                    {
                        Thread.Sleep(10000);
                    }

                    if (UpdateUsersTimerCount < 50)
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
            return tables.Users.OrderBy(user => user.Name).ThenBy(user => user.Password).AsQueryable().ToList();
        }

        public async Task<bool> Authenticate(User user)
        {
            var userLookUp = tables.Users.FirstOrDefault(record => record.ID == user.ID && record.Password == user.Password);
            
            if (userLookUp != null)
            {
                return await Task.FromResult(true);
            }
            else
            {
                Log.Information($"Service InterfaceService.cs Authenticate(): User {user} not found.");
                
                return await Task.FromResult(false);
            }
        }

		public void DeAuthenticate(User user)
        {
			// Logout code goes here.
		}

        private void UpdateUsers()
        {
            Random random = new Random();
            
            // Delete ------------------------------------------------

            int ID = random.Next(1, 100);

            var userDelete = tables.Users.FirstOrDefault(record => record.ID == ID);

            if (userDelete != null)
            {
                tables.Users.Remove(userDelete);
            }

            //TRACE
            //Log.Information($"Service InterfaceService.cs UpdateUsers(): {user} [D] {Key}");

            // Update ------------------------------------------------

            ID = random.Next(1, 100);

            var userUpdate = tables.Users.FirstOrDefault(record => record.ID == ID);

            if (userUpdate != null )
            {
                string firstUpdate = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                string lastUpdate = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                string passwordUpdate = $"{lastUpdate}{firstUpdate}";

                userUpdate.Name = $"{lastUpdate}, {firstUpdate} {DateTime.Now.ToString("HH:mm:ss")} [Update]";

                userUpdate.Password = passwordUpdate;

                tables.Update(userUpdate);
            }

            //TRACE
            //Log.Information($"Service InterfaceService.cs UpdateUsers(): {user} [U] {Key}");

            // Insert ------------------------------------------------

            User userInsert = new User()
            {
            };

            string firstInsert = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

            string lastInsert = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

            string passwordInsert = $"{lastInsert}{firstInsert}";

            userInsert.Name = $"{lastInsert}, {firstInsert} {DateTime.Now.ToString("HH:mm:ss")} [Insert]";

            userInsert.Password = passwordInsert;

            int Agency = random.Next(1, 10);

            userInsert.Agency = Agency;

            tables.Users.Add(userInsert);

            //TRACE
            //Log.Information($"Service InterfaceService.cs UpdateUsers(): {user} [I] {Key}");

            tables.SaveChangesAsync();
        }

        private void NotifyStateUpdatedUser(User user, char type) => OnUpdateUser?.Invoke(user, type);

        public event Action<User, char> OnUpdateUser;
    }
 }
