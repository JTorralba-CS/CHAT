using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Serilog;

using Standard.Models;
using System;

namespace Standard.Databases
{
    public class IMDB : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Database");
        }

        public IMDB(): base()
        {
            ChangeTracker.Tracked += ChangeTracker_Tracked;

            ChangeTracker.StateChanged += ChangeTracker_StateChanged;
        }

        private void ChangeTracker_Tracked(object sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityTrackedEventArgs e)
        {
            //throw new System.NotImplementedException();
            
            //TRACE
            //Log.Information($"Standard IMDB.cs ChangeTracker_Tracked(): {e.Entry.Entity}");
        }

        private void ChangeTracker_StateChanged(object sender, EntityStateChangedEventArgs e)
        {
            //throw new NotImplementedException();

            //TRACE
            //Log.Information($"Standard IMDB.cs ChangeTracker_StateChanged(): {e.Entry.Entity}");

            var user = e.Entry.Entity as User;

            switch (e.Entry.State)
            {
                case EntityState.Deleted:
                    NotifyStateChangedTable(user, 'D');
                    break;
                case EntityState.Modified:
                    NotifyStateChangedTable(user, 'U');
                    break;
                case EntityState.Added:
                    NotifyStateChangedTable(user, 'I');
                    break;
            }           
        }

        private void NotifyStateChangedTable(User user, char type)
        {
            //TRACE
            //Log.Information($"Standard IMDB.cs NotifyStateChangedTable(): {user.ID} {user.Name} {user.Password} {type}");

            OnChangeTable?.Invoke(user, type);
        }

        public event Action<User, char> OnChangeTable;
    }
}
