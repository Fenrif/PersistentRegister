using Microsoft.EntityFrameworkCore;
using PersistentRegister.Models;

namespace PersistentRegister.Repositories
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; }
    }
}