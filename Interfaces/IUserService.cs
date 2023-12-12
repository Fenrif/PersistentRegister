using PersistentRegister.Dtos.User;
using PersistentRegister.Models;

namespace PersistentRegister.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<bool>> DeleteAsync(Guid id);
        Task<ApiResponse<List<GetUserDto>>> GetAllAsync();
        Task<ApiResponse<GetUserDto>> GetByIdAsync(Guid id);
        Task<ApiResponse<GetUserDto>> InsertAsync(InsertUserDto newUser);
        Task<ApiResponse<GetUserDto>> UpdateAsync(UpdateUserDto updatedUser);
    }
}