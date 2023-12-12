using PersistentRegister.Models;

namespace PersistentRegister.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
         Task<ApiResponse<bool>> IsEmailUniqueAsync(string email);
    }
}