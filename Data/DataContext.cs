using Microsoft.EntityFrameworkCore;

namespace PersistentRegister.Repositories
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base (options)
        {
            
        }
    }
}