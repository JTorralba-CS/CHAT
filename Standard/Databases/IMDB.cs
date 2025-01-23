using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Serilog;
using Standard.Models;

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

            Log.Information($"Standard IMDB.cs: {e.Entry.Entity} [Tracking]");
        }

        private void ChangeTracker_StateChanged(object sender, EntityStateChangedEventArgs e)
        {
            //throw new NotImplementedException();

            var user = e.Entry.Entity as User;
            switch (e.Entry.State)
            {
                case EntityState.Deleted:
                    Log.Information($"Standard IMDB.cs: {e.Entry.Entity} [Deleted]");
                    break;
                case EntityState.Modified:
                    Log.Information($"Standard IMDB.cs: {e.Entry.Entity} [Updated]");
                    break;
                case EntityState.Added:
                    Log.Information($"Standard IMDB.cs: {e.Entry.Entity} [Inserted]");
                    break;
            }
        }
    }
}
