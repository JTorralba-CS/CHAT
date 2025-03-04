using Microsoft.EntityFrameworkCore;

namespace Standard.Databases
{
    public class DBScoped
    {
        public DBScoped()
        {
        }

        public DBContext CreateDbContext(string dbName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DBContext>();

            optionsBuilder.UseInMemoryDatabase(dbName);
            optionsBuilder.EnableSensitiveDataLogging();

            return new DBContext(optionsBuilder.Options);
        }
    }
}
