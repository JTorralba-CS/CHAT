using Microsoft.EntityFrameworkCore;

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
    }
}
