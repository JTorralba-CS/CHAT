using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

using Serilog;

using Standard.Models;

namespace Standard.Databases
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
            ChangeTracker.Tracked += ChangeTracker_Tracked;

            ChangeTracker.StateChanged += ChangeTracker_StateChanged;
        }

        private void ChangeTracker_Tracked(object sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackedEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private void ChangeTracker_StateChanged(object sender, EntityStateChangedEventArgs e)
        {
            var table = e.Entry.Entity.GetType().Name;

            var record = e.Entry.Entity;

            //TRACE
            //Log.ForContext("Folder", "ApplicationDbContext").Information($"Portal ApplicationDbContext.cs ChangeTracker_StateChanged(): {table} -> {record} [{e.Entry.State}]");

            switch (e.Entry.State)
            {
                case EntityState.Deleted:
                    switch (table.ToLower() + "s")
                    {
                        case "users":
                            NotifyStateChangedTableUsers(e, 'D');
                            break;
                        case "units":
                            NotifyStateChangedTableUnits(e, 'D');
                            break;
                    }
                    break;
                case EntityState.Modified:
                    switch (table.ToLower())
                    {
                        case "users":
                            NotifyStateChangedTableUsers(e, 'U');
                            break;
                        case "units":
                            NotifyStateChangedTableUnits(e, 'U');
                            break;
                    }
                    break;
                case EntityState.Unchanged:
                    switch (table.ToLower())
                    {
                        case "users":
                            NotifyStateChangedTableUsers(e, 'I');
                            break;
                        case "units":
                            NotifyStateChangedTableUnits(e, 'I');
                            break;
                    }
                    break;
            }
        }

        // Users --------------------------------------------------

        public DbSet<User> Users { get; set; }

        public bool UsersLocked { get; set; }

        private void NotifyStateChangedTableUsers(EntityStateChangedEventArgs e, char type)
        {
            var record = e.Entry.Entity as User;

            if (!(type == 'I' && record.Name.ToLower().Contains("update")))
            {
                //TRACE
                //Log.ForContext("Folder", "ApplicationDbContext").Information($"Portal ApplicationDbContext.cs NotifyStateChangedTableUsers(): {record} [{type}]");

                OnChangeTableUsers?.Invoke(record, type);
            }
        }

        public event Action<User, char> OnChangeTableUsers;

        // Units --------------------------------------------------

        public DbSet<Unit> Units { get; set; }

        public bool UnitsLocked { get; set; }

        private void NotifyStateChangedTableUnits(EntityStateChangedEventArgs e, char type)
        {
            var record = e.Entry.Entity as Unit;

            if (!(type == 'I' && record.Name.ToLower().Contains("update")))
            {
                //TRACE
                //Log.ForContext("Folder", "ApplicationDbContext").Information($"Portal ApplicationDbContext.cs NotifyStateChangedTableUnits(): {record} [{type}]");

                OnChangeTableUnits?.Invoke(record, type);
            }
        }

        public event Action<Unit, char> OnChangeTableUnits;
    }
}
