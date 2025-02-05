using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

using Serilog;

using Standard.Models;

namespace Standard.Databases
{
    public class IMDBX : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("DatabaseX");
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public IMDBX(): base()
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
            var user = e.Entry.Entity as User;

            //TRACE
            //Log.Information($"Standard IMDB.cs ChangeTracker_StateChanged(): {user} [{e.Entry.State}]");

            switch (e.Entry.State)
            {
                case EntityState.Deleted:
                    NotifyStateChangedTable(user, 'D');
                    break;
                case EntityState.Modified:
                    NotifyStateChangedTable(user, 'U');
                    break;
                case EntityState.Unchanged:
                    NotifyStateChangedTable(user, 'I');
                    break;
            }
        }

        private void NotifyStateChangedTable(User user, char type)
        {
            if (!(type == 'I' && user.Name.ToLower().Contains("update")))
            {
                //TRACE
                //Log.Information($"Standard IMDB.cs NotifyStateChangedTable(): {user} [{type}]");

                OnChangeTable?.Invoke(user, type);
            }
        }

        public event Action<User, char> OnChangeTable;
    }
}
