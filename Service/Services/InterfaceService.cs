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

        private System.Timers.Timer UpdateTimer;

        private int UpdateTimerCount;

        private static DBContext Database;

        private DBService DBService;

        public InterfaceService(int key)
        {
            DBService = new DBService();

            this.Key = key;

            Connection = new Dictionary<string, DateTime>();

            if (Key == 0)
            {
                if (Database == null)
                {
                    Log.Information($"Service InterfaceService.cs InterfaceService(): Database == null");

                    Database = DBService.CreateDbContext("System");
                }

                Database.OnChangeTableUsers += (user, type) =>
                {
                    NotifyStateUpdatedUser(user, type);
                };

                Database.OnChangeTableUnits += (unit, type) =>
                {
                    NotifyStateUpdatedUnit(unit, type);
                };

                UpdateTimer = new System.Timers.Timer(10000);

                UpdateTimerCount = 0;           

                UpdateTimer.Elapsed += (s, e) =>
                {
                    if (UpdateTimerCount == 0)
                    {
                        Thread.Sleep(10000);
                    }

                    if (UpdateTimerCount < 50)
                    {
                        UpdateUsers();

                        UpdateUnits();

                        UpdateTimerCount++;
                    }
                    else
                    {
                        UpdateTimer.Stop();
                    }
                };

                UpdateTimer.Start();
            }
            else
            {
            }
        }

        public List<User> GetUsers()
        {
            List<User> Users = Database.Users.OrderBy(record => record.Name).ThenBy(record => record.Password).AsQueryable().ToList();

            if (!Users.Any())
            {
                List<User> noData = new List<User>();

                noData.Add(
                    new User
                    {
                        ID = 999999,
                        Name = "N/A",
                        Password = "N/A",
                        Agency = 999999
                    }
                );

                Users = noData.ToList();
            }

            return Users;
        }

        public async Task<bool> Authenticate(User user)
        {
            var userLookUp = Database.Users.FirstOrDefault(record => record.ID == user.ID && record.Password == user.Password);
            
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

            var userDelete = Database.Users.FirstOrDefault(record => record.ID == ID);

            if (userDelete != null)
            {
                Database.Users.Remove(userDelete);
            }

            //TRACE
            //Log.Information($"Service InterfaceService.cs UpdateUsers(): {userDelete} [D] {Key}");

            // Update ------------------------------------------------

            ID = random.Next(1, 100);

            var userUpdate = Database.Users.FirstOrDefault(record => record.ID == ID);

            if (userUpdate != null )
            {
                string firstUpdate = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                string lastUpdate = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

                string passwordUpdate = $"{lastUpdate}{firstUpdate}";

                userUpdate.Name = $"{lastUpdate}, {firstUpdate} {DateTime.Now.ToString("HH:mm:ss")} [Update]";

                userUpdate.Password = passwordUpdate;

                Database.Update(userUpdate);
            }

            //TRACE
            //Log.Information($"Service InterfaceService.cs UpdateUsers(): {userUpdate} [U] {Key}");

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

            Database.Users.Add(userInsert);

            //TRACE
            //Log.Information($"Service InterfaceService.cs UpdateUsers(): {userInsert} [I] {Key}");

            Database.SaveChangesAsync();
        }

        public List<Unit> GetUnits()
        {
            List<Unit> Units = Database.Units.OrderBy(record => record.Agency).ThenBy(record => record.Jurisdiction).ThenBy(record => record.Name).ThenBy(record => record.Status).ThenBy(record => record.Location).AsQueryable().ToList();

            if (!Units.Any())
            {
                List<Unit> noData = new List<Unit>();

                noData.Add(
                    new Unit
                    {
                        ID = 999999,
                        Agency = 999999,
                        Jurisdiction = "N/A",
                        Name = "N/A",
                        Status = "N/A",
                        Location = "N/A"
                    }
                );

                Units = noData.ToList();
            }

            return Units;
        }

        private void UpdateUnits()
        {
            Random random = new Random();

            // Delete ------------------------------------------------

            int ID = random.Next(1, 100);

            var unitDelete = Database.Units.FirstOrDefault(record => record.ID == ID);

            if (unitDelete != null)
            {
                Database.Units.Remove(unitDelete);
            }

            //TRACE
            //Log.Information($"Service InterfaceService.cs UpdateUnits(): {unitDelete} [D] {Key}");

            // Update ------------------------------------------------

            ID = random.Next(1, 100);

            var unitUpdate = Database.Units.FirstOrDefault(record => record.ID == ID);

            if (unitUpdate != null)
            {
                string[] updateStatusOptions = new[]
                        {
                            "Available", "In Quarters", "Off Duty"
                        };

                int updateStatusOption = random.Next(0, 2);

                string[] updateLocationOptions = new[]
                {
                            "ABC", "DEF", "GHI", "JKL", "MNO", "PQR", "STU", "VWX"
                        };

                int updateLocationOption = random.Next(0, 7);

                unitUpdate.Status = updateStatusOptions[updateStatusOption];

                unitUpdate.Location = $"{updateLocationOptions[updateLocationOption]} {DateTime.Now.ToString("HH:mm:ss")} [Update]";

                Database.Update(unitUpdate);
            }

            //TRACE
            //Log.Information($"Service InterfaceService.cs UpdateUnits(): {unitUpdate} [U] {Key}");

            // Insert ------------------------------------------------

            int insertAgency = random.Next(1, 10);

            string[] insertJurisdictionOptions = new[]
                        {
                            "North", "North-East", "East", "South-East", "South", "South-West", "West", "North-West"
                        };

            int insertJurisdictionOption = random.Next(0, 7);

            string[] insertNamePrefixOptions = new[]
            {
                            "A", "B", "C", "X", "Y", "Z"
                        };

            int insertNamePrefixOption = random.Next(0, 5);

            string[] insertStatusOptions = new[]
            {
                            "Available", "In Quarters", "Off Duty"
                        };

            int insertStatusOption = random.Next(0, 2);

            string[] insertLocationOptions = new[]
            {
                            "ABC", "DEF", "GHI", "JKL", "MNO", "PQR", "STU", "VWX"
                        };

            int insertLocationOption = random.Next(0, 7);

            Unit unitInsert = new Unit()
            {
                Jurisdiction = insertJurisdictionOptions[insertJurisdictionOption],
                Name = $"{insertNamePrefixOptions[insertNamePrefixOption]}999",
                Status = insertStatusOptions[insertStatusOption],
                Location = $"{insertLocationOptions[insertLocationOption]} {DateTime.Now.ToString("HH:mm:ss")} [Insert]",
                Agency = insertAgency
            };

            Database.Units.Add(unitInsert);

            //TRACE
            //Log.Information($"Service InterfaceService.cs UpdateUnits(): {unitInsert} [I] {Key}");

            Database.SaveChangesAsync();
        }

        private void NotifyStateUpdatedUser(User user, char type) => OnUpdateUser?.Invoke(user, type);

        public event Action<User, char> OnUpdateUser;

        private void NotifyStateUpdatedUnit(Unit unit, char type) => OnUpdateUnit?.Invoke(unit, type);

        public event Action<Unit, char> OnUpdateUnit;
    }
 }
