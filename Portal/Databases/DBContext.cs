﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Serilog;

using Standard.Models;

namespace Portal.Databases
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
                    switch (table.ToLower())
                    {
                        case "user":
                            NotifyStateChangedTableUsers(e, 'D');
                            break;
                    }
                    break;
                case EntityState.Modified:
                    switch (table.ToLower())
                    {
                        case "user":
                            NotifyStateChangedTableUsers(e, 'U');
                            break;
                    }
                    break;
                case EntityState.Unchanged:
                    switch (table.ToLower())
                    {
                        case "user":
                            NotifyStateChangedTableUsers(e, 'I');
                            break;
                    }
                    break;
            }
        }

        private void NotifyStateChangedTableUsers(EntityStateChangedEventArgs e, char type)
        {
            var record = e.Entry.Entity as User;

            if (!(type == 'I' && record.Name.ToLower().Contains("update")))
            {
                //TRACE
                //Log.ForContext("Folder", "ApplicationDbContext").Information($"Portal ApplicationDbContext.cs NotifyStateChangedTableUsers(): {record} [{type}]");

                OnChangeTableUsers?.Invoke();
            }
        }

        public event Action OnChangeTableUsers;

        public DbSet<User> Users { get; set; }

        public bool UsersLocked { get; set; }
    }
}
