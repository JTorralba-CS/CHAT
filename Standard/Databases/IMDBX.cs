using Microsoft.EntityFrameworkCore;

using Serilog;

using Standard.Models;

namespace Standard.Databases
{
    public class IMDBX : DbContext
    {
        public DbSet<User> Users { get; set; }

        public IMDBX(DbContextOptions<IMDBX> options) : base(options)
        {
        }
    }
}
